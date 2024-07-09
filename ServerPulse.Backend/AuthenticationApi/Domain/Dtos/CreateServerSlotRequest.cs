namespace AuthenticationApi.Domain.Dtos
{
    public class CreateServerSlotRequest
    {
        public string Id { get; set; }
        public string UserEmail { get; set; }
        public string Name { get; set; }
    }
}
