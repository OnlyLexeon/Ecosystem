using UnityEngine;

public class AnimalNameGet : MonoBehaviour
{
    private static string[] cuteNames = new string[]
    {
        "Bubbles", "Marshmallow", "Sprinkle", "Tofu", "Noodle", "Pebble", "Choco", "Mochi", "Snickers", "Gummy",
        "Pudding", "Biscuit", "Pancake", "Muffin", "Cupcake", "Butter", "Waffles", "Sugar", "Jellybean", "Caramel",
        "Cheesecake", "Dumpling", "Tater Tot", "Peanut", "Oreo", "Cinnamon", "Cocoa", "Truffle", "Taffy", "Cookie",
        "Fluffy", "Fizzy", "Cuddles", "Whiskers", "Snickers", "Froggy", "Blinky", "Squeaky", "Bunbun", "Fuzzy",
        "Wiggly", "Hops", "Squishy", "Pompom", "Dizzy", "Tinsel", "Twinkle", "Glitter", "Bouncy", "Pipsqueak",
        "Poppy", "Snuggles", "Nibbles", "Chirpy", "Bobo", "Zippy", "Sunny", "Chubby", "Fizzy", "Tiki",
        "Tutu", "Doodle", "Binky", "Mittens", "Snowball", "Momo", "Plushy", "Poof", "Nana", "Giggles",
        "Glimmer", "Quacky", "Wiggles", "Bumble", "Tinsel", "Taco", "Pickles", "Boop", "Ducky", "Hiccup",
        "Bebop", "Zuzu", "Jingles", "Scooter", "Tinker", "Goober", "Dizzy", "Pookie", "Coconut", "Puddingpop",
        "Peppy", "Lulu", "Waffle", "Floofy", "Chonky", "Chiffon", "Pumpernickel", "Tootsie", "Bouncy", "Cupie"
    };

    public static string GetRandomCuteName()
    {
        return cuteNames[Random.Range(0, cuteNames.Length)];
    }
}
