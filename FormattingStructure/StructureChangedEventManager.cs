using System;
using System.Collections.Specialized;
using System.Windows;

namespace OpenFontWPFControls.FormattingStructure;

public class StructureChangedEventManager : WeakEventManager
{
    private StructureChangedEventManager()
    {
    }

    public static void AddListener(INotifyStructureChanged source, IWeakEventListener listener)
    {
        CurrentManager.ProtectedAddListener(source, listener);
    }

    public static void RemoveListener(INotifyStructureChanged source, IWeakEventListener listener)
    {
        CurrentManager.ProtectedRemoveListener(source, listener);
    }

    public static void AddHandler(INotifyStructureChanged source, EventHandler<StructureChangedEventArgs> handler)
    {
        CurrentManager.ProtectedAddHandler(source, handler);
    }

    public static void RemoveHandler(INotifyStructureChanged source, EventHandler<StructureChangedEventArgs> handler)
    {
        CurrentManager.ProtectedRemoveHandler(source, handler);
    }

    protected override ListenerList NewListenerList()
    {
        return new ListenerList<StructureChangedEventArgs>();
    }

    protected override void StartListening(object source)
    {
        INotifyStructureChanged typedSource = (INotifyStructureChanged)source;
        typedSource.StructureChanged += OnChanged;
    }

    protected override void StopListening(object source)
    {
        INotifyStructureChanged typedSource = (INotifyStructureChanged)source;
        typedSource.StructureChanged -= OnChanged;
    }


    private static StructureChangedEventManager CurrentManager
    {
        get
        {
            Type managerType = typeof(StructureChangedEventManager);
            StructureChangedEventManager manager = (StructureChangedEventManager)GetCurrentManager(managerType);

            if (manager == null)
            {
                manager = new StructureChangedEventManager();
                SetCurrentManager(managerType, manager);
            }

            return manager;
        }
    }

    private void OnChanged(object sender, StructureChangedEventArgs args)
    {
        DeliverEvent(sender, args);
    }
}