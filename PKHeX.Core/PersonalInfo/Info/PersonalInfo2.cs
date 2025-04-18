using System;

namespace PKHeX.Core;

/// <summary>
/// <see cref="PersonalInfo"/> class with values from Generation 2 games.
/// </summary>
public sealed class PersonalInfo2(Memory<byte> Raw) : PersonalInfo, IPersonalInfoTM, IPersonalInfoTutorType
{
    public const int SIZE = 0x20;

    private Span<byte> Data => Raw.Span;
    public override byte[] Write() => Raw.ToArray();

    public int DEX_ID { get => Data[0x00]; set => Data[0x00] = (byte)value; }
    public override int HP { get => Data[0x01]; set => Data[0x01] = (byte)value; }
    public override int ATK { get => Data[0x02]; set => Data[0x02] = (byte)value; }
    public override int DEF { get => Data[0x03]; set => Data[0x03] = (byte)value; }
    public override int SPE { get => Data[0x04]; set => Data[0x04] = (byte)value; }
    public override int SPA { get => Data[0x05]; set => Data[0x05] = (byte)value; }
    public override int SPD { get => Data[0x06]; set => Data[0x06] = (byte)value; }
    public override byte Type1 { get => Data[0x07]; set => Data[0x07] = value; }
    public override byte Type2 { get => Data[0x08]; set => Data[0x08] = value; }
    public override byte CatchRate { get => Data[0x09]; set => Data[0x09] = value; }
    public override int BaseEXP { get => Data[0x0A]; set => Data[0x0A] = (byte)value; }
    public int Item1 { get => Data[0xB]; set => Data[0xB] = (byte)value; }
    public int Item2 { get => Data[0xC]; set => Data[0xC] = (byte)value; }
    public override byte Gender { get => Data[0xD]; set => Data[0xD] = value; }
    public override byte HatchCycles { get => Data[0xF]; set => Data[0xF] = value; }
    public override byte EXPGrowth { get => Data[0x16]; set => Data[0x16] = value; }
    public override int EggGroup1 { get => Data[0x17] & 0xF; set => Data[0x17] = (byte)((Data[0x17] & 0xF0) | value); }
    public override int EggGroup2 { get => Data[0x17] >> 4; set => Data[0x17] = (byte)((Data[0x17] & 0x0F) | (value << 4)); }

    // EV Yields are just aliases for base stats in Gen II
    public override int EV_HP { get => HP; set { } }
    public override int EV_ATK { get => ATK; set { } }
    public override int EV_DEF { get => DEF; set { } }
    public override int EV_SPE { get => SPE; set { } }
    public override int EV_SPA { get => SPA; set { } }
    public override int EV_SPD { get => SPD; set { } }

    // Future game values, unused
    public override int GetIndexOfAbility(int abilityID) => -1;
    public override int GetAbilityAtIndex(int abilityIndex) => -1;
    public override int AbilityCount => 0;
    public override byte BaseFriendship { get => 70; set { } }
    public override int EscapeRate { get => 0; set { } }
    public override int Color { get => 0; set { } }

    private const int TMHM = 0x18;
    private const int CountTMHM = 50 + 7; // 50 TMs, 7 HMs
    private const int ByteCountTM = 8;

    public bool GetIsLearnTM(int index)
    {
        if ((uint)index >= CountTMHM)
            return false;
        return (Data[TMHM + (index >> 3)] & (1 << (index & 7))) != 0;
    }

    public void SetIsLearnTM(int index, bool value)
    {
        if ((uint)index >= CountTMHM)
            return;
        if (value)
            Data[TMHM + (index >> 3)] |= (byte)(1 << (index & 7));
        else
            Data[TMHM + (index >> 3)] &= (byte)~(1 << (index & 7));
    }

    public void SetAllLearnTM(Span<bool> result, ReadOnlySpan<byte> moves)
    {
        var span = Data.Slice(TMHM, ByteCountTM);
        if (result.Length <= Legal.MaxMoveID_1 + 1)
        {
            for (int index = CountTMHM - 1; index >= 0; index--)
            {
                if ((span[index >> 3] & (1 << (index & 7))) == 0)
                    continue;
                // If we're in a Gen1 context, we can't have Gen2 moves.
                var move = moves[index];
                if (move < result.Length)
                    result[move] = true;
            }
            return;
        }
        for (int index = CountTMHM - 1; index >= 0; index--)
        {
            if ((span[index >> 3] & (1 << (index & 7))) != 0)
                result[moves[index]] = true;
        }
    }

    private const int TutorTypeCount = 3;

    public bool GetIsLearnTutorType(int index)
    {
        if ((uint)index >= TutorTypeCount)
            return false;
        index += CountTMHM;
        return (Data[TMHM + (index >> 3)] & (1 << (index & 7))) != 0;
    }

    public void SetIsLearnTutorType(int index, bool value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual<uint>((uint)index, TutorTypeCount);
        index += CountTMHM;
        if (value)
            Data[TMHM + (index >> 3)] |= (byte)(1 << (index & 7));
        else
            Data[TMHM + (index >> 3)] &= (byte)~(1 << (index & 7));
    }

    public void SetAllLearnTutorType(Span<bool> result, ReadOnlySpan<byte> moves)
    {
        var span = Data.Slice(TMHM, ByteCountTM);
        for (int index = TutorTypeCount - 1; index >= 0; index--)
        {
            var i = index + CountTMHM;
            if ((span[i >> 3] & (1 << (i & 7))) != 0)
                result[moves[index]] = true;
        }
    }

    /// <summary>
    /// Technical Machine moves corresponding to their index within TM bitflag permissions.
    /// </summary>
    public static ReadOnlySpan<byte> MachineMoves =>
    [
        223, 029, 174, 205, 046, 092, 192, 249, 244, 237,
        241, 230, 173, 059, 063, 196, 182, 240, 202, 203,
        218, 076, 231, 225, 087, 089, 216, 091, 094, 247,
        189, 104, 008, 207, 214, 188, 201, 126, 129, 111,
        009, 138, 197, 156, 213, 168, 211, 007, 210, 171,

        015, 019, 057, 070, 148, 250, 127,
    ];

    /// <summary>
    /// Special tutor moves available via the Move Tutor (Bill's father outside Goldenrod Game Corner) added in Crystal.
    /// </summary>
    public static ReadOnlySpan<byte> TutorMoves => [(int)Move.Flamethrower, (int)Move.Thunderbolt, (int)Move.IceBeam];
}
