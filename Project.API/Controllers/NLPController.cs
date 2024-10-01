using Microsoft.AspNetCore.Mvc;
using Project.API.IServices;
using Project.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NLPController : ControllerBase
    {
        private readonly INLPService _nlpService;

        public NLPController(INLPService nlpService)
        {
            _nlpService = nlpService;
        }

        [HttpPost]
        public IActionResult ConvertToNumber(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    string output = _nlpService.WordsToNumber(value);

                    return Ok(output);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return BadRequest("Input value cannot be null or empty.");
        }

        [HttpGet]
        public IActionResult TestAllExamples()
        {
            List<string> examples = new List<string>
            {
                "Yüz bin lira kredi kullanmak istiyorum",
                "Bugün yirmi sekiz yaşına girdim",
                "Elli altı bin lira kredi alıp üç yılda geri ödeyeceğim",
                "Seksen yedi bin iki yüz on altı lira borcum var",
                "Bin yirmi dört lira eksiğim kaldı",
                "Yarın saat onaltıda geleceğim",
                "Dokuzyüzelli beş lira fiyatı var",

                // Extra examples

                "100 bin lira kullanmak istiyorum",
                "Bugün yirmi 8 yaşına girdim",
                "Elli 6 bin lira kredi alıp üç yılda geri ödeyeceğim",
                "Seksen 7 bin iki 100 on altı lira borcum var",
                "Bin 20 dört lira eksiğim kaldı",
                "Yarın saat 10altıda geleceğim",
                "9yüz50beş lira fiyatı var"
            };

            List<string> results = new List<string>();

            try
            {
                foreach (var item in examples)
                {
                    string result = _nlpService.WordsToNumber(item);
                    results.Add(result);
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
