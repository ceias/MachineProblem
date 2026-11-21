# Workflow sprite atlas

`WorkflowSprites.png` is a new generated pixel-art set created for Dream OS. It is separate from the original supplied reference artwork. The transparent atlas contains 6 columns × 5 rows (1374 × 1145 pixels). `WorkflowArt.Sprite` calculates cell boundaries from the image dimensions; no raster text, prices or controls are baked into the cards.

Zero-based row-major mapping:

| Indices | Sprites |
| --- | --- |
| 0–5 | Small / medium / large pizza; thin / thick crust; extra cheese |
| 6–11 | Mushrooms; black olives; onions; green peppers; tomatoes; soft drink |
| 12–17 | Juice; coffee; water; fan; refrigerator; washing machine |
| 18–23 | Oven toaster; iron; VCD player; mini component; television; air conditioner |
| 24–29 | Rice cooker; electric stove; electric pot; karaoke; microwave; receipt paper |

The 14 appliance sprites follow `Catalog.Products` order, starting at index 15. Each card remains a native keyboard-operable Button. The selected state and prices are drawn at runtime.

The receipt-paper image is used only for the empty slip illustration. Actual receipts are rendered from `ReceiptData`, with unique references, timestamps, itemized rows, totals and contextual footers. Save exports a branded PNG image by default, with UTF-8 text as an option. Preview, image export and Print share the same pink Dream OS layout; Print uses the Windows print dialog, including a PDF printer if installed. The decorative barcode footer is not a scannable payment code. Changing the calculation inputs clears the receipt and disables saving/printing until it is regenerated.

`ServiceSprites.png` is an additional original generated transparent pixel-art atlas, 1448 × 1086, arranged in 4 columns × 3 rows. All icons are separate from text and selection borders. Runtime indices 40–51 map to: single bed, twin beds, family room, regular room, deluxe room, suite, cash, cheque, credit card, staff ID, delivery truck, and gift voucher. Both Visual Studio app entry projects copy this sheet to the output Assets folder. Card images are owned/disposed by their controls; the shared atlas is disposed at application exit.
