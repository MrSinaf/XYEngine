<h1 align="center">

<a href="https://sinafproduction.xyz/projects/xyengine">![xy.png](assets/textures/xy.png)</a>

</h1>

<div align="center">

### ⚠️ LE MOTEUR EST ACTIVEMENT EN DÉVELOPPEMENT ⚠️

Certaines fonctionnalités peuvent ne pas fonctionner correctement ou des changements critiques peuvent être effectués entre diverses versions 0.x.0.0.

</div>

## 🐍〉Présentation

XYEngine est un moteur de jeu 2D **Code-only**.

Il est conçu pour faciliter le développement de jeux principalement procéduraux. Ce moteur n'a pas d'interface d'éditeur, car tout le processus se fait
directement via du code. Le moteur vise à simplifier l'intégration de logiques procédurales tout en permettant une grande flexibilité pour les développeurs.

## 💻〉Fonctionnalités

- **Code-Only Development** : Tout le développement se fait par code, sans aucun éditeur. Il suffit d'écrire directement la logique de jeu en C#.
- **Ressources et assets** : Chargement dynamique et multithreading des ressources via le code avec un système de mise en cache intégré.
- **UI Responsive** : Prise en charge de la création d'une interface riche utilisateur pouvant s'adapter à différentes résolutions d'écrans.
- **Rendu 2D optimisé** : XYEngine étant un moteur exclusivement 2D, il bénéficie d'une optimisation accrue sur le rendu.
- **Programmation moderne** : Utilisation de .NET9 et des fonctionnalités modernes, permettant d'avoir un code efficace et épuré.
- **ImGui intégré** : Éditeur ImGui intégré facilitant les actions de débogage en temps réel dans l'application.

## 🖱️〉Compatibilités

XYEngine a été créé et utilisé uniquement sur Windows x64. Théoriquement, il est possible de l'utiliser sur Linux et Mac.

Il reste nécessaire d'avoir [.NET 9 d'installé](https://dotnet.microsoft.com/fr-fr/download/dotnet) pour pouvoir utiliser XYEngine.

## 🚀〉Installation

1. **Créez un projet C#** .NET 9, en utilisant le modèle "Console".
2. **Ajoutez la référence** vers XYEngine :
    - Via votre IDE avec la recherche nuget : `XYEngine`
    - Via votre terminal : `dotnet add package XYEngine`
3. **Configurez votre fichier `.csproj`** en y ajoutant :

```xml
<!-- Begin XYEngine integration -->
<ItemGroup>
   <None Update="assets\**\*" CopyToOutputDirectory="Always"/>
</ItemGroup>
<Target Name="MoveDLLsInPackages" AfterTargets="Build">
    <ItemGroup>
        <LocalDlls Include="$(OutDir)*.dll" Exclude="$(OutDir)$(ProjectName).dll;$(OutDir)XYEngine.dll"/>
    </ItemGroup>
    <Move SourceFiles="@(LocalDlls)" DestinationFolder="$(OutDir)packages\%(RecursiveDir)"/>
</Target>
<!-- End XYEngine integration -->
```

> Permet d'ajouter `assets` à votre *build* et de ranger les dépendances de *votre projet* et *XYEngine* dans un dossier `packages`.

4. **Créez une nouvelle class** héritant de la class 'Scene' :

```csharp
public class MyFirstScene : Scene 
{
    // Scripts pour votre scène...
}
```

5. **Lancez votre jeu** depuis la class `Program` :

```csharp
// Lancez votre jeu en référençant le type de votre scène. (Cette scène sera la première à être affiché.)
XYEngine.XY.LaunchGame<MyFirstScene>("Nom de mon projet");
```

6. **Exécutez le projet**, si tout est correctement configuré, le moteur devrait se lancer en affichant un SplashScreen, puis un écran noir représentant votre
   scène.

> Un template prêt à l'emploi pour XYEngine est disponible [ici](https://github.com/MrSinaf/XYEngine.Template).

## 🌐〉Site web

Pour plus d'informations sur le fonctionnement du moteur découvrez sa page dédiée sur [mon site web](https://sinafproduction.xyz/projects/xyengine).