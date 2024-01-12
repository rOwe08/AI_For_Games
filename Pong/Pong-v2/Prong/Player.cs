namespace Prong
{
    public interface Player
    {
        PlayerAction GetAction(StaticState config, DynamicState state);
    }
}
