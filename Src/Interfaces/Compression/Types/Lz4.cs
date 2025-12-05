using System;
using System.Collections.Generic;
using System.Text;

namespace Nihon.Interfaces;

public class Lz4
{
    public static string UriSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$";
    static Dictionary<string, Dictionary<char, int>> BaseReverse = new Dictionary<string, Dictionary<char, int>> { };

    public delegate char GetCharFromInt(int Value);
    public static GetCharFromInt F = (Value) => Convert.ToChar(Value);

    public delegate int GetNextValue(int Index);

    private static int GetBaseValue(string Alphabet, char Character)
    {
        if (!BaseReverse.ContainsKey(Alphabet))
        {
            BaseReverse[Alphabet] = new Dictionary<char, int> { };

            for (int i = 0; i < Alphabet.Length; ++i)
                BaseReverse[Alphabet][Alphabet[i]] = i;
        }
        return BaseReverse[Alphabet][Character];
    }

    public static string CompressUri(string Input) => Compress(Input, 6, (Value) => UriSafe[Value]);

    public static string Compress(string Text, int Bits, GetCharFromInt GetChar)
    {
        if (Text == null)
            return string.Empty;

        int i, Value, ii, EnlargeIn = 2, DictionarySize = 3, NumberOfBits = 2, DataValue = 0, DataPosition = 0;

        Dictionary<string, bool> DictionaryToCreate = new Dictionary<string, bool>();
        Dictionary<string, int> Dictionary = new Dictionary<string, int>();
        StringBuilder Data = new StringBuilder();
        string ContextC = "", ContextWC = "", ContextW = "";

        for (ii = 0; ii < Text.Length; ii++)
        {
            ContextC = Text[ii].ToString();
            if (!Dictionary.ContainsKey(ContextC))
            {
                Dictionary[ContextC] = DictionarySize++;
                DictionaryToCreate[ContextC] = true;
            }

            ContextWC = ContextW + ContextC;
            if (Dictionary.ContainsKey(ContextWC))
            { ContextW = ContextWC; }

            else
            {
                if (DictionaryToCreate.ContainsKey(ContextW))
                {
                    if (Convert.ToInt32(ContextW[0]) < 256)
                    {
                        for (i = 0; i < NumberOfBits; i++)
                        {
                            DataValue <<= 1;
                            if (DataPosition == Bits - 1)
                            {
                                DataPosition = 0;
                                Data.Append(GetChar(DataValue));
                                DataValue = 0;
                            }
                            else { DataPosition++; }
                        }
                        Value = Convert.ToInt32(ContextW[0]);
                        for (i = 0; i < 8; i++)
                        {
                            DataValue = (DataValue << 1) | (Value & 1);
                            if (DataPosition == Bits - 1)
                            {
                                DataPosition = 0;
                                Data.Append(GetChar(DataValue));
                                DataValue = 0;
                            }
                            else { DataPosition++; }
                            Value >>= 1;
                        }
                    }
                    else
                    {
                        Value = 1;
                        for (i = 0; i < NumberOfBits; i++)
                        {
                            DataValue = (DataValue << 1) | Value;
                            if (DataPosition == Bits - 1)
                            {
                                DataPosition = 0;
                                Data.Append(GetChar(DataValue));
                                DataValue = 0;
                            }
                            else { DataPosition++; }
                            Value = 0;
                        }
                        Value = Convert.ToInt32(ContextW[0]);
                        for (i = 0; i < 16; i++)
                        {
                            DataValue = (DataValue << 1) | (Value & 1);
                            if (DataPosition == Bits - 1)
                            {
                                DataPosition = 0;
                                Data.Append(GetChar(DataValue));
                                DataValue = 0;
                            }
                            else { DataPosition++; }
                            Value >>= 1;
                        }
                    }
                    EnlargeIn--;
                    if (EnlargeIn == 0)
                    {
                        EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                        NumberOfBits++;
                    }
                    DictionaryToCreate.Remove(ContextW);
                }
                else
                {
                    Value = Dictionary[ContextW];
                    for (i = 0; i < NumberOfBits; i++)
                    {
                        DataValue = (DataValue << 1) | (Value & 1);
                        if (DataPosition == Bits - 1)
                        {
                            DataPosition = 0;
                            Data.Append(GetChar(DataValue));
                            DataValue = 0;
                        }
                        else { DataPosition++; }
                        Value >>= 1;
                    }

                }
                EnlargeIn--;
                if (EnlargeIn == 0)
                {
                    EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                    NumberOfBits++;
                }

                Dictionary[ContextWC] = DictionarySize++;
                ContextW = ContextC;
            }
        }

        if (ContextW != "")
        {
            if (DictionaryToCreate.ContainsKey(ContextW))
            {
                if (Convert.ToInt32(ContextW[0]) < 256)
                {
                    for (i = 0; i < NumberOfBits; i++)
                    {
                        DataValue <<= 1;
                        if (DataPosition == Bits - 1)
                        {
                            DataPosition = 0;
                            Data.Append(GetChar(DataValue));
                            DataValue = 0;
                        }
                        else { DataPosition++; }
                    }
                    Value = Convert.ToInt32(ContextW[0]);
                    for (i = 0; i < 8; i++)
                    {
                        DataValue = (DataValue << 1) | (Value & 1);
                        if (DataPosition == Bits - 1)
                        {
                            DataPosition = 0;
                            Data.Append(GetChar(DataValue));
                            DataValue = 0;
                        }
                        else { DataPosition++; }
                        Value >>= 1;
                    }
                }
                else
                {
                    Value = 1;
                    for (i = 0; i < NumberOfBits; i++)
                    {
                        DataValue = (DataValue << 1) | Value;
                        if (DataPosition == Bits - 1)
                        {
                            DataPosition = 0;
                            Data.Append(GetChar(DataValue));
                            DataValue = 0;
                        }
                        else { DataPosition++; }
                        Value = 0;
                    }
                    Value = Convert.ToInt32(ContextW[0]);
                    for (i = 0; i < 16; i++)
                    {
                        DataValue = (DataValue << 1) | (Value & 1);
                        if (DataPosition == Bits - 1)
                        {
                            DataPosition = 0;
                            Data.Append(GetChar(DataValue));
                            DataValue = 0;
                        }
                        else { DataPosition++; }
                        Value >>= 1;
                    }
                }
                EnlargeIn--;
                if (EnlargeIn == 0)
                {
                    EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                    NumberOfBits++;
                }
                DictionaryToCreate.Remove(ContextW);
            }
            else
            {
                Value = Dictionary[ContextW];
                for (i = 0; i < NumberOfBits; i++)
                {
                    DataValue = (DataValue << 1) | (Value & 1);
                    if (DataPosition == Bits - 1)
                    {
                        DataPosition = 0;
                        Data.Append(GetChar(DataValue));
                        DataValue = 0;
                    }
                    else { DataPosition++; }
                    Value >>= 1;
                }
            }
            EnlargeIn--;
            if (EnlargeIn == 0)
            {
                EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                NumberOfBits++;
            }
        }

        Value = 2;
        for (i = 0; i < NumberOfBits; i++)
        {
            DataValue = (DataValue << 1) | (Value & 1);
            if (DataPosition == Bits - 1)
            {
                DataPosition = 0;
                Data.Append(GetChar(DataValue));
                DataValue = 0;
            }
            else { DataPosition++; }
            Value >>= 1;
        }

        while (true)
        {
            DataValue <<= 1;
            if (DataPosition == Bits - 1)
            {
                Data.Append(GetChar(DataValue));
                break;
            }
            else DataPosition++;
        }
        return Data.ToString();
    }

    private struct DataStruct
    {
        public int Value, Position, Index;
    }

    public static string DecompressUri(string Input)
    {
        Input = Input.Replace(' ', '+');
        return Decompress(Input.Length, 32, (Index) => GetBaseValue(UriSafe, Input[Index]));
    }

    public static string Decompress(int Length, int Reset, GetNextValue GetNext)
    {
        Dictionary<int, string> Dictionary = new Dictionary<int, string> { };
        int Next, EnlargeIn = 4, DictionarySize = 4, NumberOfBits = 3, i, Bits, Resb, MaxPower, Power;
        int C = 0;

        string Entry = string.Empty, W;
        StringBuilder Result = new StringBuilder();
        DataStruct Data = new DataStruct
        {
            Value = GetNext(0),
            Position = Reset,
            Index = 1
        };

        for (i = 0; i < 3; i += 1)
        { Dictionary[i] = Convert.ToChar(i).ToString(); }

        Bits = 0;
        MaxPower = (int)Math.Pow(2, 2);
        Power = 1;
        while (Power != MaxPower)
        {
            Resb = Data.Value & Data.Position;
            Data.Position >>= 1;
            if (Data.Position == 0)
            {
                Data.Position = Reset;
                Data.Value = GetNext(Data.Index++);
            }
            Bits |= (Resb > 0 ? 1 : 0) * Power;
            Power <<= 1;
        }

        switch (Next = Bits)
        {
            case 0:
                Bits = 0;
                MaxPower = (int)Math.Pow(2, 8);
                Power = 1;
                while (Power != MaxPower)
                {
                    Resb = Data.Value & Data.Position;
                    Data.Position >>= 1;
                    if (Data.Position == 0)
                    {
                        Data.Position = Reset;
                        Data.Value = GetNext(Data.Index++);
                    }
                    Bits |= (Resb > 0 ? 1 : 0) * Power;
                    Power <<= 1;
                }
                C = Convert.ToInt32(F(Bits));
                break;
            case 1:
                Bits = 0;
                MaxPower = (int)Math.Pow(2, 16);
                Power = 1;
                while (Power != MaxPower)
                {
                    Resb = Data.Value & Data.Position;
                    Data.Position >>= 1;
                    if (Data.Position == 0)
                    {
                        Data.Position = Reset;
                        Data.Value = GetNext(Data.Index++);
                    }
                    Bits |= (Resb > 0 ? 1 : 0) * Power;
                    Power <<= 1;
                }
                C = Convert.ToInt32(F(Bits));
                break;
            case 2:
                return "";
        }
        Dictionary[3] = Convert.ToChar(C).ToString();
        W = Convert.ToChar(C).ToString();
        Result.Append(Convert.ToChar(C));
        while (true)
        {
            if (Data.Index > Length)
            { return string.Empty; }

            Bits = 0;
            MaxPower = (int)Math.Pow(2, NumberOfBits);
            Power = 1;
            while (Power != MaxPower)
            {
                Resb = Data.Value & Data.Position;
                Data.Position >>= 1;
                if (Data.Position == 0)
                {
                    Data.Position = Reset;
                    Data.Value = GetNext(Data.Index++);
                }
                Bits |= (Resb > 0 ? 1 : 0) * Power;
                Power <<= 1;
            }
            switch (C = Bits)
            {
                case 0:
                    Bits = 0;
                    MaxPower = (int)Math.Pow(2, 8);
                    Power = 1;
                    while (Power != MaxPower)
                    {
                        Resb = Data.Value & Data.Position;
                        Data.Position >>= 1;
                        if (Data.Position == 0)
                        {
                            Data.Position = Reset;
                            Data.Value = GetNext(Data.Index++);
                        }
                        Bits |= (Resb > 0 ? 1 : 0) * Power;
                        Power <<= 1;
                    }

                    Dictionary[DictionarySize++] = F(Bits).ToString();
                    C = DictionarySize - 1;
                    EnlargeIn--;
                    break;
                case 1:
                    Bits = 0;
                    MaxPower = (int)Math.Pow(2, 16);
                    Power = 1;
                    while (Power != MaxPower)
                    {
                        Resb = Data.Value & Data.Position;
                        Data.Position >>= 1;
                        if (Data.Position == 0)
                        {
                            Data.Position = Reset;
                            Data.Value = GetNext(Data.Index++);
                        }
                        Bits |= (Resb > 0 ? 1 : 0) * Power;
                        Power <<= 1;
                    }
                    Dictionary[DictionarySize++] = F(Bits).ToString();
                    C = DictionarySize - 1;
                    EnlargeIn--;
                    break;
                case 2:
                    return Result.ToString();
            }

            if (EnlargeIn == 0)
            {
                EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                NumberOfBits++;
            }

            if (Dictionary.ContainsKey(C))
            { Entry = Dictionary[C]; }
            else
            {
                if (C == DictionarySize)
                { Entry = W + W[0].ToString(); }
                else
                { return null; }
            }
            Result.Append(Entry);

            Dictionary[DictionarySize++] = W + Entry[0].ToString();
            EnlargeIn--;
            W = Entry;
            if (EnlargeIn == 0)
            {
                EnlargeIn = (int)Math.Pow(2, NumberOfBits);
                NumberOfBits++;
            }
        }
    }
}
