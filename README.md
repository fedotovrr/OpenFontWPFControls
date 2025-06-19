## Text controls for WPF

Specialized: Drawing color text, stylized text and very long text

Not used `System.Windows.Documents.FlowDocument` and `System.Windows.Controls.RichTextBox`

Base on  [Typography.OpenFont](https://github.com/samhocevar-forks/typography/tree/master/Typography.OpenFont) (Sorry, I don't know how to add a link, like it was done in [Emoji.WPF](https://github.com/samhocevar/emoji.wpf))

### Label 

`OpenFontWPFControls.Controls.Label` - Simple text viewer

Full rendering and drawing

### TextBox

`OpenFontWPFControls.Controls.TextBox` - Simple text writer

Rendering by paragraph in view

Scroll value is the glyph index of the first line in the view

### StructuralTextViewer

`OpenFontWPFControls.Controls.StructuralTextViewer` - Stylized text viewer

Just add interfaces from namespace `OpenFontWPFControls.FormattingStructure` for your structure or use an existing `OpenFontWPFControls.FormattingStructure.DefaultFormattingStructure` and set `FormattingStructure` property for `StructuralTextViewer`


#### Virtualizing graphic

At the moment only the `TextBox` is automatically virtualized when it is passed limited dimensions in `MeasureOverride` (not work if placed in the `StackPanel` or any unbounded control)

Drawing area can be bounded `OpenFontWPFControls.Controls.BaseTextControl.DrawingBounds` property
