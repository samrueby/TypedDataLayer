namespace TypedDataLayer.DataAccess.CommandWriting.InlineConditionAbstraction {
	/// <summary>
	/// Use at your own risk.
	/// </summary>
	public interface InlineDbCommandWithConditions {
		/// <summary>
		/// Use at your own risk.
		/// </summary>
		void AddCondition( InlineDbCommandCondition condition );
	}
}