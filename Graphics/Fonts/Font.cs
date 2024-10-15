using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace XYEngine.Graphics.Fonts;

/* TODO : Cette class est partiellement fonctionnel pour un bon usage.
 * La manière dont les mesures sont calculés ne sont pas encore garantie, le tout reste très brouillon, ce qui risque de rendre la class Label imprévisible.
 * Il reste alors conseillé de créé le Font avec un heightSize à 64.
 */
public class Font
{
    public readonly Dictionary<char, Character> characters = [];
    public readonly Texture texture;

    public readonly int lineHeight;
    
    /// <summary>
    /// Crée une texture bitmap avec les informations des characters d'une police d'écriture.
    /// </summary>
    /// <param name="fontTarget">Police ciblé relatif à "Resources/Fonts/".</param>
    /// <param name="heightSize">Taille de la police d'écriture (en hauteur).</param>
    public unsafe Font(string fontTarget, uint heightSize = 64)
    {
        FT_LibraryRec_* lib;
        FT_FaceRec_* face;

        FT_Init_FreeType(&lib);
        
        var fontBytes = File.ReadAllBytes($"Resources/Fonts/{fontTarget}.ttf");
        fixed (byte* ptr = fontBytes)
            FT_New_Memory_Face(lib, ptr, fontBytes.Length, 0, &face);

        FT_Set_Pixel_Sizes(face, heightSize, heightSize);

        lineHeight = (int)heightSize;
        texture = new Texture(1024, 1024);

        var quartHeight = lineHeight * 0.25F;   // Utilisé pour centrer les characters, le résultat est plaisant, mais semble imprévisible.
        var advance = Vector2Int.zero;
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789àâäéèêëîïôöùûüç-_/\\|+=*.,;:!?()[]{}<>\"'&%$#@*^~`ï";
        foreach (var c in chars)
        {
            FT_Load_Glyph(face, FT_Get_Char_Index(face, c), FT_LOAD.FT_LOAD_DEFAULT);
            FT_Render_Glyph(face->glyph, FT_Render_Mode_.FT_RENDER_MODE_SDF);
            
            var glyph = face->glyph;  
            var bitmap = face->glyph->bitmap;
            if (bitmap.width + advance.x >= texture.size.x)
            {
                advance.y += lineHeight + 10;
                advance.x = 0;
            }
            
            for (var x = 0; x < bitmap.pitch; x++)
            for (var y = 0; y < bitmap.rows; y++)
            {
                var @byte = bitmap.buffer[x + y * bitmap.pitch];
                texture[advance.x + x, advance.y + (int)bitmap.rows - y - 1] = new Color(@byte, @byte, @byte, @byte);
            }
            var uvCharacter = new Rect(advance.ToVector2() * texture.texelSize.x, new Vector2(bitmap.pitch + advance.x, bitmap.rows + advance.y) * texture.texelSize.y);
            characters.TryAdd(c, new Character(new Vector2(bitmap.pitch, bitmap.rows),
                                               new Vector2(bitmap.width + glyph->bitmap_left, glyph->bitmap_top - bitmap.rows + quartHeight - 5),
                                               glyph->bitmap_left,
                                               uvCharacter));

            advance.x += (int)bitmap.width + 10;
        }
        texture.ApplyPixels();
    }
}