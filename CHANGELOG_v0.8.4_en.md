[b][u]Rework[/u][/b]
[list][*][b]Blessed Scion[/b]: copies are now created by count — [b]1[/b] copy at base, [b]2[/b] when upgraded (removed the "non-upgraded copy is Exhausted" mechanic).[/list]

[b][u]Adjustment[/u][/b]
[list][*][b]Kurakura's Meals[/b]: each card is now an independent instance (multiple cards rotate, grant, and stack separately); dish Critical Stars are summed across instances (e.g. un-upgraded 20 + upgraded 30 = 50); added upgraded-form description.[/list]

[b][u]Fix (Multiplayer)[/u][/b]
[list][*][b]Noble Phantasm button[/b]: candidates are now sent via the network action payload (JSON card ID list) by the initiator; all peers parse by the same index, fixing Noble Phantasm selection desync caused by divergent NobleDeck copies.
[*][b]NobleDeck broadcast[/b]: only the deck owner broadcasts the full-pile snapshot; non-owner peers no longer override the real owner's deck.[/list]
