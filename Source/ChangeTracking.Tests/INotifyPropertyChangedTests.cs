﻿using FluentAssertions;
using System;
using System.ComponentModel;
using Xunit;

namespace ChangeTracking.Tests
{
    public class INotifyPropertyChangedTests
    {
        [Fact]
        public void AsTrackable_Should_Make_Object_Implement_INotifyPropertyChanged()
        {
            var order = Helper.GetOrder();

            Order trackable = order.AsTrackable();

            trackable.Should().BeAssignableTo<System.ComponentModel.INotifyPropertyChanged>();
        }

        [Fact]
        public void Change_Property_Should_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.CustomerNumber = "Test1";

            monitor.Should().RaisePropertyChangeFor(o => ((Order)o).CustomerNumber);
        }

        [Fact]
        public void RejectChanges_Should_Raise_PropertyChanged()
        {
            var order = Helper.GetOrder();

            var trackable = order.AsTrackable();
            trackable.Id = 963;
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();
            var intf = trackable.CastToIChangeTrackable();
            intf.RejectChanges();

            monitor.Should().RaisePropertyChangeFor(o => ((Order)o).Id);
        }

        [Fact]
        public void When_PropertyChanged_Raised_Property_Should_Be_Changed()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var inpc = (System.ComponentModel.INotifyPropertyChanged)trackable;
            int newValue = 0;
            inpc.PropertyChanged += (o, e) => newValue = order.Id;

            trackable.Id = 1234;

            newValue.Should().Be(1234);
        }

        [Fact]
        public void When_CollectionProperty_Children_Trackable_Change_Property_On_Item_In_Collection_Should_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.OrderDetails[0].ItemNo = "Testing";

            monitor.Should().RaisePropertyChangeFor(o => ((Order)o).OrderDetails);
        }

        [Fact]
        public void When_CollectionProperty_Children_Not_Trackable_Change_Property_On_Item_In_Collection_Should_Not_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable(makeCollectionPropertiesTrackable: false);
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.OrderDetails[0].ItemNo = "Testing";

            monitor.Should().NotRaisePropertyChangeFor(o => ((Order)o).OrderDetails);
        }

        [Fact]
        public void When_CollectionProperty_Children_Trackable_Change_CollectionProperty_Should_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.OrderDetails.Add(new OrderDetail
            {
                OrderDetailId = 123,
                ItemNo = "Item123"
            });

            monitor.Should().RaisePropertyChangeFor(o => ((Order)o).OrderDetails);
        }

        [Fact]
        public void When_CollectionProperty_Children_Not_Trackable_Change_CollectionProperty_Should_Not_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable(makeCollectionPropertiesTrackable: false);
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.OrderDetails.Add(new OrderDetail
            {
                OrderDetailId = 123,
                ItemNo = "Item123"
            });

            monitor.Should().NotRaisePropertyChangeFor(o => ((Order)o).OrderDetails);
        }

        [Fact]
        public void When_ComplexProperty_Children_Trackable_Change_Property_On_Complex_Property_Should_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.Address.City = "Chicago";

            monitor.Should().RaisePropertyChangeFor(o => ((Order)o).Address);
        }

        [Fact]
        public void When_Not_ComplexProperty_Children_Trackable_Change_Property_On_Complex_Property_Should_Not_Raise_PropertyChanged_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable(makeComplexPropertiesTrackable: false);
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.Address.City = "Chicago";

            monitor.Should().NotRaisePropertyChangeFor(o => ((Order)o).Address);
        }

        [Fact]
        public void Change_Property_Should_Raise_PropertyChanged_On_ChangeTrackingStatus_Event()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.CustomerNumber = "Test1";
            IChangeTrackable<Order> changeTrackable = trackable.CastToIChangeTrackable();

            monitor.Should().RaisePropertyChangeFor(ct => ct.CastToIChangeTrackable().ChangeTrackingStatus);
        }

        [Fact]
        public void Change_Property_Should_Raise_PropertyChanged_On_ChangedProperties()
        {
            var order = Helper.GetOrder();
            var trackable = order.AsTrackable();
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackable.CustomerNumber = "Test1";
            IChangeTrackable<Order> changeTrackable = trackable.CastToIChangeTrackable();

            monitor.Should().RaisePropertyChangeFor(ct => ct.CastToIChangeTrackable().ChangedProperties);
        }

        [Fact]
        public void Change_Property_From_Value_To_Null_Should_Stop_Notification()
        {
            Order trackable = new Order { Id = 321, Address = new Address { AddressId = 0 } }.AsTrackable();
            Address trackableAddress = trackable.Address;
            trackable.Address = null;
            var monitor = ((INotifyPropertyChanged)trackable).Monitor();

            trackableAddress.AddressId = 2;

            monitor.Should().NotRaisePropertyChangeFor(o => ((Order)o).Address);
        }

        [Fact]
        public void PropertyChanged_On_Circular_Reference_Should_Not_Throw_OverflowException()
        {
            var update0 = new InventoryUpdate
            {
                InventoryUpdateId = 0
            };
            var update1 = new InventoryUpdate
            {
                InventoryUpdateId = 1,
                LinkedToInventoryUpdate = update0
            };
            update0.LinkedInventoryUpdate = update1;

            var trackable = update0.AsTrackable();
            //read these properties to force event wire up
            _ = trackable.LinkedInventoryUpdate.LinkedToInventoryUpdate;
            trackable.InventoryUpdateId = 3;
        }
    }
}
