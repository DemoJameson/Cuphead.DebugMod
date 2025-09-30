# What is BeePlatforms.txt
This is a file that can be used to inject a platform pattern into the Rumor Honeybottoms fight. This can be done by enabling `Read Missing Platforms from file` under `RNG Rumor Honeybottoms`, and placing a file called `BeePlatforms.txt` in the same folder as the Cuphead executable.

# File Format
At its core, the file format is pretty simple: each row corresponds to the index of the one platform that's supposed to go missing, bottom up.

Each row is made up of either 3 or 4 platforms. So, depending on the row, this index (which is zero-index) can either contain values between 0 and 2, or between 0 and 3.

For example, take the following file:
```
0
1
0
3
2
```

This would result in the 1st platform going missing, then the 2nd, then the 1st, then the 4th, then the 3rd.
The platforms are numbered from left to right, meaning that the "1st platform" means the one that's farthest left, the "2nd platform" means the one just right of it, and so on.
Once the file has been read to the end, the rest of the platforms generated in the fight will be random.

That being said, there are a few complicated rules that can result in the game not actually removing any platform from certain rows.

# How the game's code randomizes platforms

First of all, when the camera has scrolled far enough up into the stage, the game will generate **4 new rows of platforms**. That's right, platform generation is essentially done in chunks, 4 rows at a time.
Initially these rows will be completely filled with platforms.
Then, if the difficulty is set to Regular or Expert, the game will iterate over these 4 rows, and one by one, it will attempt to remove 1 platform from each row.
The code that does this on a single row is located in `BeeLevelPlatforms.Randomize()` and looks like this:

```
public void Randomize(int missingCount)
	{
		foreach (Transform transform in this.rows)
		{
			List<Transform> list = new List<Transform>(transform.GetChildTransforms());
			foreach (Transform transform2 in list)
			{
				transform2.gameObject.SetActive(true);
			}
			for (int j = 0; j < missingCount; j++)
			{
				if (list.Count <= 1)
				{
					break;
				}
				int num = UnityEngine.Random.Range(0, list.Count);
				if (num == 0 && BeeLevelPlatforms.lastPlatform == 0)
				{
					break;
				}
				if (num == 3 && BeeLevelPlatforms.lastPlatform == 2)
				{
					break;
				}
				list[num].gameObject.SetActive(false);
				BeeLevelPlatforms.lastPlatform = num;
				list.RemoveAt(num);
			}
		}
	}
```

There's a few observations to be made here:
* First of all, while this code technically supports the ability to remove more than 1 platform per row, in reality this method always gets called with `missingCount = 1`, meaning that always 1 platform per row will go missing.
* The for loop is contains a couple conditions that will interrupt this method early if said conditions are met, skipping the removal of the platform (`list[num].gameObject.SetActive(false);`):
  * **1st Condition**: If the platform it attempted to remove is the 1st one, and the last platform removed was also the 1st one, then no platforms will be removed this time.
  * **2nd Condition**: If the platform it attempted to remove is the 3rd one, and the last platform removed was the 2nd, then no platforms will be removed this time (also leaving `lastPlatform` at 2).

To add more complexity to it all, the 4 rows that spawn for each chunk are iterated on **from top to bottom**.
If you think about it, this means that the game doesn't actually always work on rows that are right next to each other. Here's for example the order that the game works on the first 8 rows of platforms of the fight. On the left is the order the player will actually encounter the row during the fight, from the bottom to the top. On the right is the order the game actually follows to attempt to remove a platform from each row:

| Order Encountered  | Order the game generates the missing platform |
| ------------------ | --------------------------------------------- |
| 1st                | 4th                                           |
| 2nd                | 3rd                                           |
| 3rd                | 2nd                                           |
| 4th                | 1st                                           |
| 5th                | 8th                                           |
| 6th                | 7th                                           |
| 7th                | 6th                                           |
| 8th                | 5th                                           |

This implies that between the 4th and the 5th rows encountered, you might actually get two "1st > 1st" platforms missing for example, because when the time comes to generate the 5th row, the `lastPlatform` field will have the value of whatever platform will be missing in the **6th** row, not the **4th** row.

# BeePlatforms file example with some of these rules applied

Take the following file:
```
0
3
2
0
0
2
0
0
```

First of all, this file is split into chunks of 4 rows each, and are read in reverse order.
* The 4th line is the one that gets read first, and whichever platform index is here will go missing in the 4th row encountered, which in this case is the **1st**.
* The 3rd line gets read next, and this results in the **3rd** platform going missing in the 3rd row encountered.
* The 2nd line is now read. This attempts to get the **4th** platform removed in the 2nd row encountered, but because the previous platform that was just removed was the 3rd one, no platforms will actually be removed due to the **2nd Condition**.
* The 1st line is finally read. This results in the **1st** platform being removed in the 1st row encountered.
* The 8th line is read next. Because this attempts to remove the **1st** platform again, no platforms will be removed due to the **1st Condition**, even though this is being applied on the 8th row encountered, far away from the row that was just previously worked on which was the 1st one.
* The 7th line is read next. This tries to remove the **1st** platform again, which due to the **1st Condition**, results in no platforms going missing in the 7th row encountered.
* The 6th line is read next, this removes the **3rd** platform from the 6th row encountered.
* The 5th line is read last. This removes the **1st** platform from the 5th row encountered. This means that the 4th and 5th row encountered will both have the 1st platform missing!

So in practice, if we were to represent with a dash (-) a row in which a platform did not go missing, here's what the file would actually look like:
```
0
-
2
0
0
2
-
-
```
