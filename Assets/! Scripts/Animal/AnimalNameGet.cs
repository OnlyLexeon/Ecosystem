using UnityEngine;

public class AnimalNameGet : MonoBehaviour
{
    private static string[] cuteNames = new string[]
    {
        "Bubbles", "Marshmallow", "Sprinkle", "Tofu", "Noodle", "Pebble", "Choco", "Mochi", "Snickers", "Gummy",
        "Pudding", "Biscuit", "Pancake", "Muffin", "Cupcake", "Butter", "Waffles", "Sugar", "Jellybean", "Caramel",
        "Cheesecake", "Dumpling", "Tater Tot", "Peanut", "Oreo", "Cinnamon", "Cocoa", "Truffle", "Taffy", "Cookie",
        "Fluffy", "Fizzy", "Cuddles", "Whiskers", "Froggy", "Blinky", "Squeaky", "Bunbun", "Fuzzy",
        "Wiggly", "Hops", "Squishy", "Pompom", "Dizzy", "Tinsel", "Twinkle", "Glitter", "Bouncy", "Pipsqueak",
        "Poppy", "Snuggles", "Nibbles", "Chirpy", "Bobo", "Zippy", "Sunny", "Chubby", "Tiki",
        "Tutu", "Doodle", "Binky", "Mittens", "Snowball", "Momo", "Plushy", "Poof", "Nana", "Giggles",
        "Glimmer", "Quacky", "Wiggles", "Bumble", "Tinsel", "Taco", "Pickles", "Boop", "Ducky", "Hiccup",
        "Bebop", "Zuzu", "Jingles", "Scooter", "Tinker", "Goober", "Dizzy", "Pookie", "Coconut", "Puddingpop",
        "Peppy", "Lulu", "Waffle", "Floofy", "Chonky", "Chiffon", "Pumpernickel", "Tootsie", "Bouncy", "Cupie",
        "Peaches", "Cuddlepuff", "Jellybean", "Sprout", "Lollipop", "Gingersnap", "Mallow", "Snickerdoodle",
        "Honeybun", "Twix", "Marzipan", "Doodlebug", "Buttercup", "Fritter", "Mochaccino", "Cinnabun", "Macaron",
        "Snicker", "Fuzzball", "Mopsy", "Poppyseed", "Nutmeg", "Clover", "Basil", "Blueberry", "Raspberry",
        "Lemon Drop", "Sugarplum", "Puddingcup", "Tangerine", "Frosting", "Jujube", "Bubblegum", "Gingersnap",
        "Carrotcake", "Strudel", "Dandy", "Skittles", "Button", "Jellyroll", "ChocoChip", "Tater", "Meringue",
        "Nuzzle", "Fizgig", "Bibbles", "Whiffles", "Goofball", "Tiddlywink", "Gigglet", "Hobnob", "Huggles",
        "Boopsy", "Cinnapuff", "Snookie", "Honeysuckle", "Marmalade", "S’mores",
        "Wigglebottom", "Toffy", "Puff", "Butterbean", "Crumpet", "Fizpuff", "Snugglebug", "Snoot",
        "Puffball", "Chubbybun", "Frosty", "Nutter", "Lolly", "Syrup", "Dottie", "Bouncyboo", "Toodles",
        "Cheeky", "Fizwhiz", "Bobbles", "Snoodle", "Dandybun", "Jujubee", "Sprinkles", "Dizzywig", "Taterbean"
    };


    public static string GetRandomCuteName()
    {
        return cuteNames[Random.Range(0, cuteNames.Length)];
    }
}
