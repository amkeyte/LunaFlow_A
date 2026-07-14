namespace LunaFlow.Engine
{
    /// <summary>
    /// A translator is an adapter that allows engine access from different types of services. For example there will be a
    /// Console translator, an IP translator, possibly a GUI translator, probably a Test translator etc.
    /// The game engine is a singleton instance. Therefore, GameEngine will start running when the first translator accesses it's instance.
    /// 
    /// </summary>
    public interface IGameEngineTranslator
    {
        //add stuff here
    }
}
