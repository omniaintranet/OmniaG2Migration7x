using Omnia.Fx.Models.Language;
using Omnia.Fx.Models.TargetingFilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Links
{
    public class LinkIconTypes
    {
        public static readonly string FontAwesome = "IFontAwesomeIcon";
        public static readonly string Fabric = "IFabricIcon";
        public static readonly string Flag = "IFlagIcon";
        public static readonly string Custom = "ICustomIcon";
    }

    public class LinkIcon
    {
        public string iconType { get; set; }
        public string faClass { get; set; }
        public string customValue { get; set; }
        public string color{ get; set; }
        public string backgroundColor { get; set; }
    }

    public class QuickLink
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the user login.
        /// </summary>
        /// <value>
        /// The name of the user login.
        /// </value>
        public string UserLoginName { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public MultilingualString Title { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the information.
        /// </summary>
        /// <value>
        /// The information.
        /// </value>
        public MultilingualString Information { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>
        /// The icon.
        /// </value>
        public dynamic Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is open new window.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is open new window; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpenNewWindow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is owner.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is owner; otherwise, <c>false</c>.
        /// </value>
        public bool IsOwner { get; set; }

        /// <summary>
        /// get or sets a value of bussinessprofile
        /// </summary>
        public Guid? BusinessProfileId { get; set; }

        /// <summary>
        /// get or set Mandatory link
        /// </summary>
        public bool Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the modified.
        /// </summary>
        /// <value>
        /// The modified.
        /// </value>
        public DateTimeOffset ModifiedAt { get; set; }

        public TargetingFilterData TargetingFilter { get; set; }

        public bool? IsTargeted { get; set; }
    }
}
