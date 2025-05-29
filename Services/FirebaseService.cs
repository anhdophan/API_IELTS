using Firebase.Database;

namespace api.Services
{
    public static class FirebaseService
    {
        public static readonly FirebaseClient Client;

        static FirebaseService()
        {
            try
            {
                Client = new FirebaseClient("https://ielts-7d51b-default-rtdb.asia-southeast1.firebasedatabase.app/");
            }
            catch (Exception ex)
            {
                Console.WriteLine("FirebaseClient init error: " + ex.Message);
                throw;
            }
        }
    }
}