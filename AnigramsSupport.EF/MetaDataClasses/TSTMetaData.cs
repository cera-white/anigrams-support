using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace TST.DATA.EF
{
    #region TSTProjectMetaData
    public class TSTProjectMetaData
    {
        [Required(ErrorMessage = "*Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must be less than 1000 characters.")]
        [DisplayFormat(NullDisplayText = "No description.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "*Category is required")]
        [Display(Name = "Category")]
        public int ProjectCategoryID { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(TSTProjectMetaData))]
    public partial class TSTProject { }
    #endregion

    #region TSTProjectCategoryMetaData
    public class TSTProjectCategoryMetaData
    {
        [Required(ErrorMessage = "*Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(250, ErrorMessage = "Description must be less than 250 characters.")]
        [DisplayFormat(NullDisplayText = "No description.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(TSTProjectCategoryMetaData))]
    public partial class TSTProjectCategory { }
    #endregion

    #region TSTUserMetaData
    public class TSTUserMetaData
    {
        [Required(ErrorMessage = "*First Name is required")]
        [MaxLength(25, ErrorMessage = "Name must be less than 25 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "*Last Name is required")]
        [MaxLength(25, ErrorMessage = "Name must be less than 25 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        [DisplayFormat(NullDisplayText = "[None Selected]")]
        public string FullName { get; }

        [Required(ErrorMessage = "*Title is required")]
        [Display(Name = "Title")]
        public int UserTitleID { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime StartDate { get; set; }

        [ScaffoldColumn(false)]
        [DisplayFormat(NullDisplayText = "Current", DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> EndDate { get; set; }

        [MaxLength(100, ErrorMessage = "Address must be less than 100 characters.")]
        [DisplayFormat(NullDisplayText = "No address listed")]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }

        [MaxLength(50, ErrorMessage = "Address must be less than 50 characters.")]
        [DisplayFormat(NullDisplayText = "No address listed")]
        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; }

        [MaxLength(50, ErrorMessage = "City must be less than 50 characters.")]
        [DisplayFormat(NullDisplayText = "No city listed")]
        public string City { get; set; }

        [MaxLength(100, ErrorMessage = "Region must be less than 100 characters.")]
        [DisplayFormat(NullDisplayText = "No region listed")]
        [Display(Name = "State/Region")]
        public string State { get; set; }

        [MaxLength(20, ErrorMessage = "Zip code must be less than 20 characters.")]
        [DisplayFormat(NullDisplayText = "No postal code listed")]
        [Display(Name = "Postal Code")]
        public string Zip { get; set; }

        [MaxLength(100, ErrorMessage = "Country must be less than 100 characters.")]
        [DisplayFormat(NullDisplayText = "No country listed")]
        public string Country { get; set; }

        [Required(ErrorMessage = "*Email is required")]
        [RegularExpression(@"^[0-9a-zA-Z]+([0-9a-zA-Z]*[-._+])*[0-9a-zA-Z]+@[0-9a-zA-Z]+([-.][0-9a-zA-Z]+)*([0-9a-zA-Z]*[.])[a-zA-Z]{2,6}$", ErrorMessage = "Please use a valid email address (example@domain.com)")]
        public string Email { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> DateOfBirth { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(TSTUserMetaData))]
    public partial class TSTUser
    {
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
    #endregion

    #region TSTUserTitleMetaData
    public class TSTUserTitleMetaData
    {
        [Required(ErrorMessage = "*Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(250, ErrorMessage = "Description must be less than 250 characters.")]
        [DisplayFormat(NullDisplayText = "No description.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(TSTUserTitleMetaData))]
    public partial class TSTUserTitle { }
    #endregion

    #region TSTTicketMetaData
    public class TSTTicketMetaData
    {
        [Required(ErrorMessage = "Subject is required")]
        [MaxLength(75, ErrorMessage = "Subject must be less than 75 characters.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Classification is required")]
        [Display(Name = "Classification")]
        public int TicketClassificationID { get; set; }

        [Display(Name = "Submitted By")]
        public int SubmittedBy { get; set; }

        [Display(Name = "Assigned To")]
        [DisplayFormat(NullDisplayText = "N/A")]
        public Nullable<int> TechAssigned { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public int TicketStatusID { get; set; }

        [Display(Name = "Date Submitted")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime DateSubmitted { get; set; }

        [Display(Name = "Date Resolved")]
        [DisplayFormat(NullDisplayText = "N/A", DataFormatString = "{0:d}")]
        public Nullable<System.DateTime> DateResolved { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        public int ProjectID { get; set; }

        [MaxLength(500, ErrorMessage = "Tags cannot exceed 500 characters.")]
        [DisplayFormat(NullDisplayText = "No keywords listed")]
        public string Tags { get; set; }

        [DisplayFormat(NullDisplayText = "N/A")]
        public Nullable<int> ResolutionID { get; set; }
    }

    [MetadataType(typeof(TSTTicketMetaData))]
    public partial class TSTTicket { }
    #endregion

    #region TSTTicketClassificationMetaData
    public class TSTTicketClassificationMetaData
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(250, ErrorMessage = "Description must be less than 250 characters.")]
        [DisplayFormat(NullDisplayText = "No description.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Range(1,3, ErrorMessage = "Priority Level must be a number between {1} and {2}")]
        [Required(ErrorMessage = "Priority Level is required")]
        [Display(Name = "Priority Level")]
        public int? PriorityLevel { get; set; }
    }

    [MetadataType(typeof(TSTTicketClassificationMetaData))]
    public partial class TSTTicketClassification { }
    #endregion

    #region TSTTicketStatusMetaData
    public class TSTTicketStatusMetaData
    {
        [Required(ErrorMessage = "*Name is required")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters.")]
        public string Name { get; set; }

        [MaxLength(250, ErrorMessage = "Description must be less than 250 characters.")]
        [DisplayFormat(NullDisplayText = "No description.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    [MetadataType(typeof(TSTTicketStatusMetaData))]
    public partial class TSTTicketStatus { }
    #endregion

    #region TSTCommentMetaData
    public class TSTCommentMetaData
    {
        [Required(ErrorMessage = "*Comment is required")]
        [MaxLength(500, ErrorMessage = "Comment must be less than 500 characters.")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        [Required]
        public int PostedBy { get; set; }

        [Required]
        public System.DateTime DatePosted { get; set; }

        [Required]
        public int TicketID { get; set; }
    }

    [MetadataType(typeof(TSTCommentMetaData))]
    public partial class TSTComment { }
    #endregion

    #region TSTResolutionMetaData
    public class TSTResolutionMetaData
    {
        [Required(ErrorMessage = "Problem is required")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Problem { get; set; }

        [Required(ErrorMessage = "Resolution is required")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Resolution { get; set; }

        [Required]
        public int PostedBy { get; set; }

        [Required]
        public System.DateTime DatePosted { get; set; }
    }

    [MetadataType(typeof(TSTResolutionMetaData))]
    public partial class TSTResolution { }
    #endregion

    #region TSTUserPreferences
    public class TSTUserPreferencesMetaData
    {
        [Required(ErrorMessage = "Display Name is required")]
        public string DisplayName { get; set; }
    }

    [MetadataType(typeof(TSTUserPreferencesMetaData))]
    public partial class TSTUserPreference { }
    #endregion

    #region TSTUserNotifications
    public class TSTUserNotificationsMetaData
    {
        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime DateEntered { get; set; }
    }

    [MetadataType(typeof(TSTUserNotificationsMetaData))]
    public partial class TSTUserNotification { }
    #endregion

    #region TSTMessages
    public class TSTMessagesMetaData
    {
        [Required(ErrorMessage = "You must choose a recipient")]
        public int Recipient { get; set; }

        [Required(ErrorMessage = "Subject Required")]
        [MaxLength(75, ErrorMessage = "Subject must be less than 75 characters")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message Required")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        public System.DateTime DateSent { get; set; }
    }

    [MetadataType(typeof(TSTMessagesMetaData))]
    public partial class TSTMessage { }
    #endregion
}
