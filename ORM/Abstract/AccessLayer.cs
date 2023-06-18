using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM.Abstract;

public abstract class AccessLayer
{
	public abstract string Name { get; }

	public abstract string ConnectionString { get; }
}
