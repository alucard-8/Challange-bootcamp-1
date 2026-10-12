namespace Soulbreak.Core
{
    // Anything the player can click-shoot (enemies, orbs). The shooter only knows this interface.
    public interface IShootable
    {
        void Shoot(int damage);
    }
}
