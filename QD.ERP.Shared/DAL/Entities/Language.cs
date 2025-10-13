using System;
using System.Collections.Generic;

namespace QD.ERP.Shared.DAL.Entities
{
    public class Language
    {
        public int Id { get; set; }   // Primary key (auto-incremented)
        public string Name { get; set; }  // Name of the language (e.g., "English", "Arabic")
        public string Value { get; set; } // Language code (e.g., "en", "ar")
        public string Flag { get; set; }
    }

}
