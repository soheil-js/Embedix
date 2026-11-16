# Embedix GUI

Embedix GUI is a Windows Forms application for embedding dependencies without using the command line. It requires Windows and the .NET 10 Desktop Runtime. For macOS and Linux use the [CLI](cli.md) or [EmbedixStudio](https://github.com/soheil-js/EmbedixStudio).

## Using the GUI

1. **Choose the assembly.** Click **Browse...** or drag an `.exe` / `.dll` onto the file box. For modern .NET applications choose the `.dll`.
2. **Choose the dependencies.**
   - Tick **Automatically** to let Embedix find the referenced DLLs beside your assembly, or
   - Leave it unticked and use **Add** (multi-select) or drag `.dll` files into the list. **Clear** empties the list.
3. **Optionally untick Write Pdb** if you do not want a PDB file.
4. Click **Run**. A message box reports success or that no libraries were selected.

## Output

The processed assembly is written to an **`Embedix`** folder next to the input file. If a file with the same name already exists there it is replaced.

For a `.dll` input that has both a same-name `.exe` and `.runtimeconfig.json` beside it, both files are also copied into the `Embedix` folder.

## Help menu

| Item | Action |
| --- | --- |
| Latest Release | Opens the GitHub releases page |
| Source Code | Opens the repository |
| About | Shows the application and Core versions |

## Tips

- Add each library once. Embedding two files with the same name creates conflicting resources.
- Do not process an already processed file again; always start from your original build output.
- For automation, use the [CLI](cli.md).
