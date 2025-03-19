using UnityEngine;
using Zenject;

namespace DragAndDropTestCase
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private LayerMask _draggableLayer;
        [SerializeField] private SpriteRenderer _background;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TouchInput>()
                .FromComponentInHierarchy()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<InputHandler>()
                .AsSingle()
                .WithArguments(_draggableLayer, _background)
                .NonLazy();
        }
    }
}