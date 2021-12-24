﻿
namespace ME.ECS.Tests {

    public class UnmanagedComponentsStorageTests {

        public struct TestComponent {

            public int value;

        }
        
        [NUnit.Framework.Test]
        [NUnit.Framework.RepeatAttribute(20)]
        public void Initialize() {

            var reg = new UnmanagedComponentsStorage();
            reg.Initialize();
            try {
                WorldUtilities.InitComponentTypeId<TestComponent>();
                reg.Validate<TestComponent>();
                for (int i = 0; i < 100; ++i) {
                    reg.Validate<TestComponent>(i);
                }
            } finally {
                reg.Dispose();
            }

        }

        [NUnit.Framework.TestAttribute]
        [NUnit.Framework.RepeatAttribute(20)]
        public void Set() {

            var reg = new UnmanagedComponentsStorage();
            reg.Initialize();
            try {
                WorldUtilities.InitComponentTypeId<TestComponent>();
                reg.Validate<TestComponent>();
                for (int i = 0; i < 100; ++i) {
                    reg.Validate<TestComponent>(i);
                }

                reg.Set(1, new TestComponent() {
                    value = 123,
                });
                var data = reg.Read<TestComponent>(1);
                NUnit.Framework.Assert.AreEqual(data.value, 123);
            } finally {
                reg.Dispose();
            }

        }

        [NUnit.Framework.TestAttribute]
        [NUnit.Framework.RepeatAttribute(20)]
        public void Read() {

            var reg = new UnmanagedComponentsStorage();
            reg.Initialize();
            try {
                WorldUtilities.InitComponentTypeId<TestComponent>();
                reg.Validate<TestComponent>();
                for (int i = 0; i < 100; ++i) {
                    reg.Validate<TestComponent>(i);
                }

                var data = reg.Read<TestComponent>(1);
                NUnit.Framework.Assert.AreEqual(data.value, 0);
                var data2 = reg.Read<TestComponent>(2);
                NUnit.Framework.Assert.AreEqual(data2.value, 0);
            } finally {
                reg.Dispose();
            }

        }

        [NUnit.Framework.TestAttribute]
        [NUnit.Framework.RepeatAttribute(20)]
        public void Get() {

            var reg = new UnmanagedComponentsStorage();
            reg.Initialize();
            try {
                WorldUtilities.InitComponentTypeId<TestComponent>();
                reg.Validate<TestComponent>();
                for (int i = 0; i < 100; ++i) {
                    reg.Validate<TestComponent>(i);
                }

                ref var data = ref reg.Get<TestComponent>(1);
                NUnit.Framework.Assert.AreEqual(data.value, 0);
                var data2 = reg.Get<TestComponent>(2);
                NUnit.Framework.Assert.AreEqual(data2.value, 0);
                data.value = 123;
                var data3 = reg.Get<TestComponent>(1);
                NUnit.Framework.Assert.AreEqual(data3.value, 123);

            } finally {
                reg.Dispose();
            }

        }

        [NUnit.Framework.TestAttribute]
        [NUnit.Framework.RepeatAttribute(20)]
        public void Has() {

            var reg = new UnmanagedComponentsStorage();
            reg.Initialize();
            try {
                WorldUtilities.InitComponentTypeId<TestComponent>();
                reg.Validate<TestComponent>();
                for (int i = 0; i < 100; ++i) {
                    reg.Validate<TestComponent>(i);
                }

                reg.Set(1, new TestComponent());
                NUnit.Framework.Assert.True(reg.Has<TestComponent>(1));
                NUnit.Framework.Assert.False(reg.Has<TestComponent>(2));

            } finally {
                reg.Dispose();
            }

        }

    }

}