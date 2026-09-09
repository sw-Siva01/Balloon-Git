using Colyseus.Schema;

public class BalloonRoomState : Schema
{
    [Type(0, "uint16")] public ushort playerCount = 0;
    [Type(1, "array", typeof(ArraySchema<byte>), "uint8")] public ArraySchema<byte> history = new ArraySchema<byte>();
}
