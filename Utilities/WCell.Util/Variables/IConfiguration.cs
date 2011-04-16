using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Variables
{
    public interface IConfiguration
    {
        bool Load();

        void Save(bool backup, bool auto);

        bool Contains(string name);

        bool IsReadOnly(string name);

        object Get(string name);

        bool Set(string name, string value);

        bool Set(string name, object value);

        void Foreach(Action<IVariableDefinition> callback);
    }
}