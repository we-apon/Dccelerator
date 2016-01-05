using System;


namespace Dccelerator.DataAccess {

    /// <summary>
    /// Criterion that can and will be used for data filtering
    /// </summary>
    public interface IDataCriterion {

        /// <summary>
        /// Name of criterion. Usually it's name of storred procedure parameter, or data table collumn name.
        /// </summary>
        
        string Name { get; set; }

        /// <summary>
        /// Value of cruterion.
        /// </summary>
        
        object Value { get; set; }

        /// <summary>
        /// Type of <see cref="Value"/> of criterion.
        /// </summary>
        
        Type Type { get; set; }
    }
}