using System;

namespace VibeChat.Web
{
    public static class BackgroundColors
    {
        /// <summary>
        ///     array of colors that are set by default to profile's circle background
        /// </summary>
        public static string[] ProfilePicColors =
        {
            "#6EC9CB", // dim Blue
            "#3c4361", // special violet
            "#ee7aae", // dim pink
            "#7bc862", // dim green
            "#eda86c", // dim orange
            "#000000", // black
            "#a695e7" // dim violet
        };

        /// <summary>
        ///     array of colors that are set by default to group's circle background
        /// </summary>
        public static string[] GroupBackgroundColors =
        {
            "Black",
            "Orange",
            "Red",
            "Dimgray",
            "Darkblue",
            "Pink"
        };

        /// <summary>
        ///     Get random background for profile's circle background
        /// </summary>
        /// <returns></returns>
        public static string GetProfilePicRgb()
        {
            var randomColor = new Random();
            return ProfilePicColors[randomColor.Next(0, ProfilePicColors.Length - 1)];
        }

        /// <summary>
        ///     Get random background for group's circle background
        /// </summary>
        /// <returns></returns>
        public static string GetGroupBackground()
        {
            var randomColor = new Random();
            return GroupBackgroundColors[randomColor.Next(0, GroupBackgroundColors.Length - 1)];
        }
    }
}