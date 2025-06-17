using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vertikal.Core.Helpers
{
    public static class FirebaseUrls
    {
        public const string FirestoreBase = "https://firestore.googleapis.com/v1/projects/vertikal-911f5/databases/(default)/documents";
        
        public const string SummitsCollection = FirestoreBase + "/Summits";
        public const string UsersCollection = FirestoreBase + "/Users";
        public const string AscentsCollection = FirestoreBase + "/Ascents";


    }
}
