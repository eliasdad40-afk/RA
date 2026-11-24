le dejo el ultimo problema que tuvimos con ndk: 
Encontré la causa y la ruta válida: el NDK dentro de Unity 6000.6.8f1 está incompleto,
pero en Unity 6000.5.7f1 está instalado el NDK r27c exacto que requiere tu editor (27.2.12479018).
En **Edit > Preferences > External Tools > Android**, desmarca solo **Android NDK Tools Installed with Unity (recommended)** y
apunta el campo **NDK** a `C:\Program Files\Unity\Hub\Editor\6000.5.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\NDK`. 
