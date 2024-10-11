using System.Runtime.InteropServices;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace XYEngine.Graphics.Fonts;

/* TODO : Cette class n'est pas encore entièrement fonctionnel pour un bon usage.
 * Il manque le fait de pouvoir choisir la police d'écriture, mais aussi sa taille.
 * Ensuite, la manière dont les mesures sont calculés ne sont pas encore garantie, le tout est très brouillon, ce qui rend la class Label imprévisible.
 */
public class Font
{
    public readonly Dictionary<char, Character> characters = [];
    public readonly Texture texture;

    public readonly int lineHeight;
    
    public unsafe Font()
    {
        FT_LibraryRec_* lib;
        FT_FaceRec_* face;

        FT_Init_FreeType(&lib);
        FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi("Resources/Fonts/JetBrainsMono-Bold.ttf"), 0, &face);
        FT_Set_Char_Size(face, 0, 8 * 64, 300, 300);

        texture = new Texture(1024, 1024);
        
        var advance = 0;
        var advanceY = 0;
        const float ratio = 1 / 1024F;
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789àâäéèêëîïôöùûüç-_/\\|+=*.,;:!?()[]{}<>\"'&%$#@*^~`ï";
        foreach (var c in chars)
        {
            if (c == ' ')
            {
                advance += 10;
                continue;
            }
            
            FT_Load_Glyph(face, FT_Get_Char_Index(face, c), FT_LOAD.FT_LOAD_DEFAULT);
            FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_NORMAL);
            var bitmap = face->glyph->bitmap;
            var height = (int)bitmap.rows;
            if (bitmap.width + advance >= texture.size.x)
            {
                advanceY += lineHeight + 10;
                advance = 0;
                lineHeight = 0;
            }

            if (lineHeight < height)
            {
                lineHeight = height;
                Console.WriteLine(c);
            }
            
            for (var x = 0; x < bitmap.width; x++)
            for (var y = 0; y < height; y++)
            {
                var @byte = bitmap.buffer[x + y * bitmap.pitch];
                texture[advance + x, advanceY + height - y - 1] = new Color(255, 255, 255, @byte);
            }
            characters.TryAdd(c, new Character(new Vector2(bitmap.width, bitmap.rows), new Vector2(bitmap.width, face->glyph->bitmap_top - bitmap.rows), 
                                               new Rect(new Vector2(advance,  advanceY) * ratio, new Vector2(bitmap.width + advance, bitmap.rows + advanceY) * ratio)));
            advance += (int)bitmap.width + 10;
        }
        texture.ApplyPixels();
    }
}