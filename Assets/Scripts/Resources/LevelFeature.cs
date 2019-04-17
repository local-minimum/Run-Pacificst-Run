using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LevelFeatureValue = System.UInt32;

public enum GroundType { BASIC, BLOCKING_MOVABLE, BLOCKING_IMOVABLE, NONBLOCKING_MOVABLE };
public enum AgentType { PLAYER, MONSTER, NPC, ENEMY_PLAYER };

public static class LevelFeature
{
    static readonly LevelFeatureValue One = 1;
    static readonly LevelFeatureValue Zero = 0;

    #region GROUND
    static readonly ushort GroundIdBits = 3;

    public static LevelFeatureValue Ground(bool movable, bool obstruction, ushort identifier)
    {
        if (identifier > (One << GroundIdBits))
        {
            throw new System.NotSupportedException($"The identifier must be in the range 0 - 5 (not {identifier})");
        }
        return (movable ? One : Zero) | (obstruction ? One << 1 : Zero) | ((LevelFeatureValue)identifier << 2);
    }

    static readonly LevelFeatureValue SemanticGroundMatch = 0b11;

    public static bool FulfillsSemanticGroundMask(bool movable, bool obstruction, LevelFeatureValue value)
    {
        LevelFeatureValue mask = (movable ? One : Zero) | (obstruction ? One << 1 : Zero);
        return ((~(mask ^ value)) & SemanticGroundMatch) == SemanticGroundMatch;
    }

    static readonly LevelFeatureValue ObstructionMask = 0b01;
    public static bool IsBlocked(LevelFeatureValue value) 
    {
        //TODO: Could be blocked by object
        return (value & ObstructionMask) == ObstructionMask;
    }

    public static GroundType GetGroundType(LevelFeatureValue value)
    {
        bool movable = (One & value) == One;
        bool obstruction = ((One << 1) & value) == (One << 1);
        if (movable)
        {
            return obstruction ? GroundType.BLOCKING_MOVABLE : GroundType.NONBLOCKING_MOVABLE;
        } else
        {
            return obstruction ? GroundType.BLOCKING_IMOVABLE : GroundType.BASIC;
        }
    }

    static readonly LevelFeatureValue GroundIdMask = 0b00111;

    public static bool FullfillsGroundMask(ushort identifier, LevelFeatureValue value)
    {
        LevelFeatureValue mask = (((LevelFeatureValue)identifier) << 2);
        return ((~(mask ^ value)) & GroundIdMask) == GroundIdMask;
    }

    static readonly LevelFeatureValue NotGroundMask = 0b00000000000011111111111111111111;
    //                                                  0   4   8   12  16  20  24  28  32
    public static LevelFeatureValue SetGround(bool movable, bool obstruction, LevelFeatureValue value)
    {
        LevelFeatureValue semanticGround = (movable ? One : Zero) | (obstruction ? One << 1 : Zero);
        LevelFeatureValue groundId = GroundIdMask & value;
        LevelFeatureValue nonground = value & NotGroundMask;
        return semanticGround | groundId | nonground;
    }
    #endregion

    #region OBJECTS
    static readonly ushort FirstObjectBit = 12;
    public static bool HasObject(LevelFeatureValue value)
    {
        return ((One << FirstObjectBit) & value) > 0;
    }
    #endregion

    #region AGENTS
    static readonly ushort FirstAgentIdBit = 27;
    static readonly ushort AgentIdBits = 5;    
    static readonly LevelFeatureValue NotAgentMask =  0b11111111111111111111111100000000;
    //                                                  0   4   8   12  16  20  24  28  32
    static readonly LevelFeatureValue HasAgentMask =  0b00000000000000000000000010000000;
    static readonly LevelFeatureValue IsPlayerMask =  0b00000000000000000000000001000000;
    static readonly LevelFeatureValue IsHostileMask = 0b00000000000000000000000000100000;
    static readonly LevelFeatureValue AgentIdMask =   0b00000000000000000000000000011111;

    public static bool HasAgent(LevelFeatureValue value)
    {
        return (HasAgentMask & value) == HasAgentMask;
    }

    public static LevelFeatureValue SetAgent(bool isPlayer, bool isHostile, ushort identifier, LevelFeatureValue value)
    {
        if (identifier > One << AgentIdBits)
        {
            throw new System.NotSupportedException($"The identifier must be in the range 0 - 5 (not {identifier})");
        }
        LevelFeatureValue agent = HasAgentMask | (isPlayer ? IsPlayerMask : Zero) | (isHostile ? IsHostileMask : Zero) | ((LevelFeatureValue)identifier << (FirstAgentIdBit));
        LevelFeatureValue notagent = NotAgentMask & value;
        return agent | notagent;
    }

    public static LevelFeatureValue ClearAgent(LevelFeatureValue value)
    {
        return value & NotAgentMask;
    }

    public static AgentType GetAgentType(LevelFeatureValue value)
    {
        bool hasPlayer = (IsPlayerMask & value) == IsPlayerMask;
        bool isHostile = (IsHostileMask & value) == IsHostileMask;
        if (hasPlayer)
        {
            return isHostile ? AgentType.ENEMY_PLAYER : AgentType.PLAYER;
        } else
        {
            return isHostile ? AgentType.MONSTER : AgentType.NPC;
        }
    }

    public static ushort GetAgentId(LevelFeatureValue value)
    {
        return (ushort) ((value & AgentIdMask) >> FirstAgentIdBit);
    }

    public static LevelFeatureValue CopyAgent(LevelFeatureValue from, LevelFeatureValue to)
    {
        LevelFeatureValue agent = (~NotAgentMask) & from;
        LevelFeatureValue notAgent = NotAgentMask & to;
        return notAgent | agent;
    }

    #endregion

    #region MIXED
    public static bool IsVacant(LevelFeatureValue value)
    {
        return !IsBlocked(value) && !HasObject(value) && !HasAgent(value);
    }
    #endregion
}

