<h1 align="center">

<a href="https://sinafproduction.xyz/projects/xyengine">![xy.png](assets/textures/xy.png)</a>

</h1>

<div align="center">

### ⚠️ LE MOTEUR EST ACTIVEMENT EN DÉVELOPPEMENT ⚠️

Certaines fonctionnalités peuvent ne pas fonctionner correctement ou des changements critiques peuvent être effectués entre diverses versions 0.x.0.0.

</div>

## 🐍〉Présentation

XYEngine est un moteur de jeu 2D **Code-only**.

Il est conçu pour faciliter le développement de jeux principalement procéduraux. Ce moteur n'a pas d'interface d'éditeur, car tout le processus se fait directement via du code. Le
moteur vise à simplifier l'intégration de logiques procédurales tout en permettant une grande flexibilité pour les développeurs.

## 💻〉Fonctionnalités

- **Code-Only Development** : Tout le développement se fait par code, sans aucun éditeur. Il suffit d'écrire directement la logique de jeu en C#.
- **Ressources et assets** : Chargement dynamique des ressources via le code avec un système de mise en cache intégré.
- **UI Responsive** : Prise en charge de la création d'une interface utilisateur pouvant s'adapter à différentes résolutions d'écrans.
- **Rendu 2D optimisé** : XYEngine étant un moteur exclusivement 2D, il bénéficie d'une optimisation accrue sur le rendu.
- **Programmation moderne** : Utilisation de .NET9 et des fonctionnalités modernes, permettant d'avoir du code efficace et épuré.

## 🖱️〉Compatibilités

XYEngine a été créé et utilisé uniquement sur Windows x64. Théoriquement, il est possible de l'utiliser sur Linux, car les dépendances sont multiplateformes.

Il reste nécessaire d'avoir [.NET 9 d'installé](https://dotnet.microsoft.com/fr-fr/download/dotnet) pour pouvoir utiliser XYEngine.

## 🚀〉Installation

1. **Téléchargez** [la dernière version](https://github.com/MrSinaf/XYEngine/releases/latest/download/XYEngine.zip) de XYEngine.
2. **Décompressez** le fichier `.zip` et placer le dossier `/XYEngine` dans l'emplacement de votre choix.

## 🌠〉Utilisation

1. **Créez un projet C#** .NET 9, en utilisant le modèle "Console".
2. **Configurez** votre fichier `.csproj` en y ajoutant :

```xml
<!-- Begin XYEngine integration -->
<PropertyGroup>
    <XYEnginePath>Chemin/vers/les/fichiers/de/XYEngine</XYEnginePath>
</PropertyGroup>
<ItemGroup>
    <None Update="assets\**\*" CopyToOutputDirectory="Always"/>
    <Reference Include="XYEngine" HintPath="$(XYEnginePath)\XYEngine.dll"/>
    <XYEngineFiles Include="$(XYEnginePath)\**\*" Visible="false"/>
</ItemGroup>
<Target Name="CopyXYEngineFiles" AfterTargets="Build">
    <Copy SourceFiles="@(XYEngineFiles)" DestinationFolder="$(OutputPath)%(RecursiveDir)" SkipUnchangedFiles="true"/>
    <Delete Condition="'$(Configuration)' == 'Release'" Files="$(OutputPath)XYEngine.pdb"/>
</Target>
<!-- End XYEngine integration -->
```

> **Note** : Remplacer `Chemin/vers/les/fichiers/de/XYEngine` par le chemin absolu ou relatif où vous avez placé le dossier XYEngine.

3. **Créez une nouvelle class** héritant de la class 'Scene' :

```csharp
public class MyFirstScene : Scene 
{
    // A vous d'ajouter les logiques de votre scène ici.
}
```

4. **Lancez votre jeu** depuis la class `Program` :

```csharp
// Lancez votre jeu en référençant le type de votre scène. (Cette scène sera la première à être affiché.)
XYEngine.XY.LaunchGame<MyFirstScene>("Nom de mon projet");
```

5. **Exécutez le projet**, si tout est correctement configuré, le moteur devrait se lancer en affichant un SplashScreen, puis un écran noir représentant votre scène.

> Un template prêt à l'emploi pour XYEngine est disponible [ici](https://github.com/MrSinaf/XYEngine.Template).

## 🌐〉Site web

Découvrez la page dédiée à ce moteur de jeu sur [mon site web](https://sinafproduction.xyz/projects/xyengine).