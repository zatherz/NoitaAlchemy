using System;
using System.Collections.Generic;
using System.Net;

namespace NoitaPRNGServer
{
	public class SaphireRandom
	{
		public double Seed;

		public double Next()
		{
			Seed = ((int)Seed) * 16807 + ((int)Seed) / 127773 * -0x7fffffff;
			if (Seed < 0) Seed += int.MaxValue;
			return Seed / int.MaxValue;
		}
	}

	public static class Recipes
	{
		public static SaphireRandom WorldRandom = new SaphireRandom();
		public static double WORLD_SEED;

		public static string[] LiquidMaterials = {
			"water",
			"water_ice",
			"water_swamp",
			"oil",
			"alcohol",
			"swamp",
			"mud",
			"blood",
			"blood_fungi",
			"blood_worm",
			"radioactive_liquid",
			"cement",
			"acid",
			"lava",
			"urine",
			"poison",
			"magic_liquid_teleportation",
			"magic_liquid_polymorph",
			"magic_liquid_random_polymorph",
			"magic_liquid_berserk",
			"magic_liquid_charm",
			"magic_liquid_invisibility"
		};

		public static string[] AlchemyMaterials = {
			"sand",
			"bone",
			"soil",
			"honey",
			"slime",
			"snow",
			"rotten_meat",
			"wax",
			"gold",
			"silver",
			"copper",
			"brass",
			"diamond",
			"coal",
			"gunpowder",
			"gunpowder_explosive",
			"grass",
			"fungi"
		};

		public static string[] GetRecipes(double seed, bool full = false) {
			WORLD_SEED = seed;
			WorldRandom.Seed = ((double)WORLD_SEED) * 0.17127000 + 1323.59030000;
			WorldRandom.Next();

			WorldRandom.Next();
			WorldRandom.Next();
			WorldRandom.Next();
			WorldRandom.Next();
			WorldRandom.Next();

			Console.WriteLine($"WORLD SEED: {WORLD_SEED}");

			var recipes = new string[2];
			recipes[0] = GetRandomRecipe("LC", full);
			recipes[1] = GetRandomRecipe("AP", full);
			return recipes;
		}

		public static void ChooseRandomMaterials(List<string> target, string[] material_list, int iters)
		{
			for (var i = 0; i < iters; i++)
			{
				var pick = material_list[(int)(WorldRandom.Next() * material_list.Length)];
				if (target.Contains(pick)) {
					i -= 1;
					continue;
				}
				target.Add(pick);
			}
		}

		public static string GetRandomRecipe(string name, bool full)
		{
			var mats = new List<string>();

			ChooseRandomMaterials(mats, LiquidMaterials, 3);
			ChooseRandomMaterials(mats, AlchemyMaterials, 1);

            var probability = WorldRandom.Next();
			WorldRandom.Next();

            probability = (10 - (int)(probability * -91.0f));

            Shuffle(mats);
			if (!full && mats.Count == 4) mats.RemoveAt(mats.Count - 1);

			if (full && mats.Count == 4) return $"{name},{probability},{mats[0]},{mats[1]},{mats[2]},{mats[3]};";
			else return $"{name},{probability},{mats[0]},{mats[1]},{mats[2]};";
		}

		public static void Shuffle(List<string> ary)
		{
			var prng = new SaphireRandom();
			prng.Seed = ((int)WORLD_SEED >> 1) + 0x30f6;
			prng.Next();

			for (var i = ary.Count - 1; i >= 0; i--)
			{
				var swap_idx = (int)(prng.Next() * (i + 1));
				var elem = ary[i];
				ary[i] = ary[swap_idx];
				ary[swap_idx] = elem;
			}
		}
	}

	class MainClass {
		public static void ErrorOutput(HttpListenerResponse response) {
			var response_str = "Forbidden\nDon't even try messing with this lmao";
			var buffer = System.Text.Encoding.UTF8.GetBytes(response_str);
			response.ContentLength64 = buffer.Length;
			response.ContentType = "text/plain";
			response.StatusCode = 403;
			using (var s = response.OutputStream) {
				s.Write(buffer, 0, buffer.Length);
				s.Close();
			}
		}

		public static void Main(string[] args) {
			var listener = new HttpListener();
			listener.Prefixes.Add("http://*:4921/");
			listener.Start();
			while (true) {
				var ctx = listener.GetContext();
				var req = ctx.Request;

				var response = ctx.Response;
				response.AddHeader("Access-Control-Allow-Origin", "*");

				try
				{
					var func = ctx.Request.Url.PathAndQuery;
					if (!func.StartsWith("/noita?"))
					{
						ErrorOutput(response);
						continue;
					}

					var query = ctx.Request.Url.Query.Substring(1);

					var split = query.Split('&');
					if (split.Length != 2)
					{
						ErrorOutput(response);
						continue;
					}

					var seed = double.Parse(split[0]);

					if (split[1] != "hey_you_reading_this_you_will_find_literally_nothing_and_just_waste_your_time") throw new Exception("a");

					var recipes = string.Join("", Recipes.GetRecipes(seed));

					var buffer = System.Text.Encoding.UTF8.GetBytes(recipes);
					response.ContentLength64 = buffer.Length;
					response.ContentType = "text/plain";
					using (var s = response.OutputStream)
					{
						s.Write(buffer, 0, buffer.Length);
						s.Close();
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					ErrorOutput(response);
				}
			}
		}
	}
}
