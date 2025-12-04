namespace Zeighty.Emulator;

public class DecodedInstruction
{
    private Z80Instruction _instruction;
    private ushort _address;
    private string _decoded;
    private string _decodedBytes;
    
    public Z80Instruction Instruction { get => _instruction; }
    public string Decoded { get => _decoded;}
    public string DecodedBytes { get => _decodedBytes; }
    public ushort Address { get => _address; }
    public DecodedInstruction(ushort Address, Z80Instruction Instruction, string Decoded, string DecodedBytes)
    {
        _address = Address;
        _instruction = Instruction;
        _decoded = Decoded;
        _decodedBytes = DecodedBytes;
    }
}
