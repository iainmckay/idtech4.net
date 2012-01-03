# Reduce number of instances where keywords are parsed in materials.
# Remove DrawWindow from UI.  Seems dirty.  Lots of, if(dw.simp) { // do x } else { // do blah }.
# idDict will suffer serious performance issues.  Converts values back and forward between strings.
# idWindow.Activate - rename to show/hide to make it more winform like.
# idWindow, idMaterial have their own EmitOp/Expression engines.  Consolodate them.
# Check; is using Rectangle (int based) rather than float based rectangle going to cause issues?