namespace AssetInteraction
{
    public interface IPlayerInteractable
    {
        string InteractionPrompt { get; }
        void Interact();
    }
}