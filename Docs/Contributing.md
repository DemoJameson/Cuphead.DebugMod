# Contributing
Contributions of any kind are welcome! This can include features or bug fixes, but also documentation etc.

You can contribute by forking the project and opening a pull request. If you're new to this sort of thing, [here](https://github.com/firstcontributions/first-contributions/blob/main/README.md)'s a pretty good introduction to the process.

If making code contributions, it is recommended to use Visual Studio and open the project solution with it.

# What is BepInEx?
BepInEx is a C# modding framework for Unity.
It does not modify the game files directly. Instead, it uses Harmony to patch the game at runtime after it gets launched and loaded into memory.

# So how do I mod the game's code?
The primary way is to use Harmony `Prefix`, `Postfix`, or `ILManipulator` methods.
`Prefix` methods will run **prior* to the execution of one of the game's original methods.
`Postfix` methods will run **after* the execution of one of the game's original methods.
`ILManipulator` methods will alter a game's original method. This involves editing the game's IL code instead of tinkering with C# code, making these methods considerably harder to use.

Cuphead's code has been compiled into [IL](https://en.wikipedia.org/wiki/Common_Intermediate_Language) language. It also keeps most symbols (object, method, property etc. names) and by using a tool like [dnSpy](https://github.com/dnSpy/dnSpy/releases) it is possible to reverse this compilation to get something similar (though not exactly) to the original source code. You can use this to browse the game's code (most of it is inside of `Assembly-CSharp.dll`) and take a look at how a lot of things are programmed much more easily than the majority of other games.

# Postfix and Prefix methods and an example
As an example, let's write a quick mod that forces the Devil's first Head attack of the fight to always be Dragon instead of Spider.
By browsing Cuphead's code with dnSpy and looking through the classes related to the Devil's level, we will eventually find that there exists a `isSpiderAttackNext` field inside of the `DevilLevelSittingLevel` class that sounds important. And in fact, by using the Ctrl+F search function and looking for other mentions of this field within this class, we will find that this field's value is checked inside of the `StartHead()` method inside of this same class, which determines whether the code for the spider of the dragon attack is run.
```
public void StartHead()
{
	this.state = DevilLevelSittingDevil.State.Head;
	if (this.isSpiderAttackNext)
	{
		base.StartCoroutine(this.spider_cr());
	}
	else
	{
		base.StartCoroutine(this.dragon_cr());
	}
	this.isSpiderAttackNext = !this.isSpiderAttackNext;
}
```
We will also notice that inside of the `LevelInit()` method, which is a method that will run at the start of the level, this field is set to a random value.
```
public override void LevelInit(LevelProperties.Devil properties)
{
	base.LevelInit(properties);
	this.isSpiderAttackNext = Rand.Bool();
	[...]
}
```
So all we need to do to achieve our goals is to run a little bit of code that runs after this `LevelInit()` method, and set the `isSpiderAttackNext` field to be `false`, basically overriding the game's `Rand.Bool()` call.

Let's create a `Postfix` method for this.

First, if you're creating a new class make sure to tag it with the `[HarmonyPatch]` attribute.
```
[HarmonyPatch]
internal class DevilPatternSelector {

}
```

Then, start writing your method. You should tag it with some attributes that will indicate which of the game's original methods Harmony will have to look for, and what type of method we're trying to write (`Prefix`, `Postfix`, or `ILManipulator`). The format for these attributes is the following:
```
[HarmonyPatch(typeof(ClassName), nameof(ClassName.MethodName))]
[HarmonyPostfix]
```
The method itself will have to be `static void`. You can name it whatever you want, and for Prefix and Postfix methods they will have to accept a `ref ClassName __instance` parameter that will grant access to the instance of the object that we're trying to hijack.
Here's for example how we can write our method. As a reminder, we are trying to write a `Postfix` method to run some code after `DevilLevelSittingDevil.LevelInit()`:
```
[HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
[HarmonyPostfix]
public static void DragonAttackManipulator(ref DevilLevelSittingDevil __instance) {
    
}
```

Finally, inside this method we can finally set the object's `isSpiderAttackNext` to false:
```
__instance.isSpiderAttackNext = false;
```

Here what our full class will look like:
```
[HarmonyPatch]
internal class DevilPatternSelector {

    [HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
    [HarmonyPostfix]
    public static void DragonAttackManipulator(ref DevilLevelSittingDevil __instance) {
        __instance.isSpiderAttackNext = false;
    }
}
```

If the method you're trying to hijack is an Enumerator or a Constructor, you will have to specify that in the attribute tags:
```
[...]
[HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.dragon_cr), MethodType,Enumerator)]
[HarmonyPostfix]
[...]
```

Also, if you want to edit one of the parameters that are being passed to the same method you're trying to hijack, you can just add a parameter to your own method that is an object of the same type and of the same name. This will let Harmony give you access to that parameter:
```
public static void DragonAttackManipulator(ref DevilLevelSittingDevil __instance, LevelProperties.Devil properties) {
    [...]
}
```

### ILManipulator methods
These are used whenever you want to actually edit the game's code. Using `Postfix` and `Prefix` methods is always recommended over this whenever you can, because `ILmanipulator` methods require editing the game's IL instead of working with C#, which is a different language that is much harder.
If you must use this, you can inspect a method's original IL code with dnSpy by right clicking inside a method's area and selecting `Edit IL Instructions...`.
Then, when you write your `ILManipulator` method, you will want to tag it as such and pass a `ILContext il` parameter.
```
[HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.bumper_car_cr), MethodType.Enumerator)]
[HarmonyILManipulator]
private static void PhaseOneBumperDelayManipulator(ILContext il) {
    [...]
}
```
Inside this method, you can create an `ILCursor` object from the `ILContext`, and use it to look for a specific IL instruction by filtering it by OpCode and Operand. Then, you can make your edits by removing or emitting new opcodes. For example:
```
[HarmonyPatch(typeof(ClownLevelClown), nameof(ClownLevelClown.bumper_car_cr), MethodType.Enumerator)]
[HarmonyILManipulator]
private static void PhaseOneBumperDelayManipulator(ILContext il) {
    ILCursor ilCursor = new(il);
    while (ilCursor.TryGotoNext(MoveType.Before, i => i.OpCode == OpCodes.Stfld && i.Operand.ToString().Contains("timerIndex"))) {
        ilCursor.EmitDelegate<Func<int, int>>(dash => GetDashPattern());
        ilCursor.Index++; // avoid infinite loops
    }
}
static int GetDashPattern() {
    return 9;
}
```
I would recommend taking a look at a few examples throughout the project to check out a few ways you can use the `HarmonyILManipulator`.

# Adding a new UI option and checking for user values
All UI options are located in `Config/Settings.cs`. You can add a new user setting here.
Let's for example try to add a new setting that allows us to select between the Dragon or Spider attack on the Devil. We'll modify the Postfix example from earlier to dynamically choose either one depending on what the user wants.
First, declare a new static field of type `ConfigEntry<type>`. The type between <> will depend on what kind of value you want to allow. For example, if you want a binary value, use a `bool` and this will create a checkbox in the UI. If you want a string, you can use a `string` and this will create a textbox in the UI.
In our case, we want a list of predetermined values. To do that, we will first create a new Enum in `Config/SettingsEnum.cs`:
```
public enum DevilPhaseOneHeadTypes {
    Random,
    Dragon,
    Spider
}
```
Then, we will use this enum as our type for `ConfigEntry`:
```
public static ConfigEntry<DevilPhaseOneHeadTypes> DevilPhaseOneHeadType;
```
Farther down in this class, we will initialize this field like the following:
```
DevilPhaseOneHeadType = config.Bind("RNG The Devil", "Phase One Head Type", DevilPhaseOneHeadTypes.Random, --order);
```
Here's what all of this means:
* `"RNG The Devil"` is the section title under which this setting will show up in the UI;
* `"Phase One Head Type"` is the name of the setting itself as it will appear in the UI;
* `DevilPhaseOneHeadTypes.Random` is the default value this setting will have;
* `--order` is just a way to organize the order of the settings within the same section.

Now that we have this setting, we can check for its value whenever we hijack Cuphead's code. Here's how we can modify the Head attack code to check for the setting we just created:
```
[HarmonyPatch(typeof(DevilLevelSittingDevil), nameof(DevilLevelSittingDevil.LevelInit))]
[HarmonyPostfix]
public static void PhaseOneHeadManipulator(ref DevilLevelSittingDevil __instance) {
    if (DevilPhaseOneHeadType.Value != DevilPhaseOneHeadTypes.Random) {
         __instance.isSpiderAttackNext = DevilPhaseOneHeadType.Value == DevilPhaseOneHeadTypes.Spider ? true : false;
    }
}
```
This will make it so that, upon the start of the Devil level:
* If the user setting has been set to Random we do nothing;
* If it's been set to a different value, we will set `isSpiderAttackNext` to true if the user selected `Spider`, and `false` if they selected anything else (aka Dragon).

# Building and Testing
Once you have something you want to test, you can go and ahead build the mod by selecting `Build > Build Solution` within Visual studio. Make sure to also select the version of the game you want to build the mod for within the dropdown just below (should be a choice between `1.0~1.1`, `1.2`, and `1.3+`).
The mod will come in the form of a `Cuphead.DebugMod.dll` inside of the `bin` folder. You can drag and drop this inside of `BepInEx/plugins` inside of your Cuphead Directory, and it should be ready to test!

# Enabling Debug Console
It is recommended to enable the Debug Console while testing a new mod. To do so, locate `BepInEx/config/BepInEx.cfg` inside of the Cuphead directory where BepInEx is installed, and open this file with any text editor. Then, scroll down and look for where it says:
```
[Logging.Console]

## Enables showing a console for log output.
# Setting type: Boolean
# Default value: false
Enabled = false
```
and change the last line to `Enabled = true`. Now, next time you boot up cuphead with BepInEx, it should launch with a debug console that will print debug information including any Error messages.

# Additional Documentation
* [BepInEx](https://docs.bepinex.dev/)
* [Harmony](https://harmony.pardeike.net/articles/intro.html)
