## Text controls for WPF

Specialized: Drawing color text, stylized text and very long text

Not used `System.Windows.Documents.FlowDocument` and `System.Windows.Controls.RichTextBox`

All that is in the host is **graphics**.

Base on  [Typography.OpenFont](https://github.com/samhocevar-forks/typography/tree/master/Typography.OpenFont) (Sorry, I don't know how to add a link, like it was done in [Emoji.WPF](https://github.com/samhocevar/emoji.wpf))

### Label 

`OpenFontWPFControls.Controls.Label` - Simple text viewer

Full rendering and drawing

### TextBox

`OpenFontWPFControls.Controls.TextBox` - Simple text writer, looks like a standard box

### LargeTextBox

`OpenFontWPFControls.Controls.LargeTextBox` - Large text writer

Rendering by paragraph in view

Scroll value is the glyph index of the first line in the view

Scroll maximum is the chars count

### StructuralTextViewer

`OpenFontWPFControls.Controls.StructuralTextViewer` - Stylized text viewer

Just add interfaces from namespace `OpenFontWPFControls.FormattingStructure` for your structure or use an existing `OpenFontWPFControls.FormattingStructure.DefaultFormattingStructure` and set `FormattingStructure` property for `StructuralTextViewer`.


### Bounds graphic

When placing in the `StackPanel` or any unbounded control, drawing bounds do not work.

Drawing area can be bounded `OpenFontWPFControls.Controls.BaseTextControl.DrawingBounds` property. See example unlimited panel `OpenFontWPFControls.Controls.ItemsPanel` (need to change the type `PART_ContentHost` in the template from `ScrollViewer` to `ContentControl`).
