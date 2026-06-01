#### Play Mode Saver ###

## To Use:

First make sure UnityPlayModeSaver.cs is inside a folder called 'Editor' somewhere in your projects Assets folder.

Then whilst the game is playing in the editor either drag any GameObject / Component onto the Play Mode Saver window
or right click on any GameObject / Component in the Inspector Panel or hierarchy view and select 
'Save Play Mode Changes' or 'Save Play Mode Snapshot'.

You can undo/re-apply any saved changes with Ctrl-Z and Ctrl-Y after leaving Play Mode.

If an object has a snapshot saved it will keep those values upon leaving Play Mode*. 
Otherwise objects will keep the values they had when leaving Play Mode.


* note this means you can save objects even after their scenes have unloaded during a play session.


