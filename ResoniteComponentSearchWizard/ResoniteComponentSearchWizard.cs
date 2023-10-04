using System.Collections.Generic;
using ResoniteModLoader;
using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using Elements.Assets;

namespace ResoniteComponentSearchWizard
{
	public class ResoniteComponentSearchWizard : ResoniteMod
	{
		public override string Name => "Component Search Wizard";
		public override string Author => "Nytra";
		public override string Version => "1.1.0";
		public override string Link => "https://github.com/Nytra/ResoniteComponentSearchWizard";

		const string WIZARD_TITLE = "Component Search Wizard (Mod)";

		public override void OnEngineInit()
		{
			Engine.Current.RunPostInit(AddMenuOption);
		}
		void AddMenuOption()
		{
			DevCreateNewForm.AddAction("Editor", WIZARD_TITLE, (x) => ComponentSearchWizard.GetOrCreateWizard(x));
		}

		class ComponentSearchWizard
		{
			public static ComponentSearchWizard GetOrCreateWizard(Slot x)
			{
				return new ComponentSearchWizard(x);
			}
			Slot WizardSlot;

			readonly ReferenceField<Slot> searchRoot;
			readonly ReferenceField<Component> componentField;

			readonly ValueField<bool> ignoreGenericTypes;
			readonly ValueField<bool> showDetails;
			//readonly ValueField<bool> confirmDestroy;
			readonly ValueField<string> nameField;
			readonly ValueField<bool> matchCase;
			//readonly ValueField<bool> allowChanges;
			readonly ValueField<bool> searchNiceName;
			readonly ValueField<int> maxResults;
			readonly ValueField<bool> exactMatch;

			readonly ReferenceMultiplexer<Component> results;

			//readonly Button destroyButton;
			readonly Button searchButton;
			//readonly Button enableButton;
			//readonly Button disableButton;

			bool performingOperations = false;

			readonly Text statusText;
			void UpdateStatusText(string info)
			{
				statusText.Content.Value = info;
			}

			string GetSlotParentHierarchyString(Slot slot, bool reverse = true)
			{
				string str;
				List<Slot> parents = new List<Slot>();

				slot.ForeachParent((parent) =>
				{
					parents.Add(parent);
				});

				if (reverse)
				{
					str = "";
					parents.Reverse();
					bool first = true;
					foreach (Slot s in parents)
					{
						if (first)
						{
							str += s.Name;
							first = false;
						}
						else
						{
							str += "/" + s.Name;
						}
					}
					if (first)
					{
						str += slot.Name;
						first = false;
					}
					else
					{
						str += "/" + slot.Name;
					}

				}
				else
				{
					str = slot.Name;
					foreach (Slot s in parents)
					{
						str += "/" + s.Name;
					}
				}

				return str;
			}

			bool ValidateWizard()
			{
				if (searchRoot.Reference.Target == null)
				{
					UpdateStatusText("No search root provided!");
					return false;
				}

				//if (componentField.Reference.Target == null)
				//{
				//UpdateStatusText("No component provided!");
				//return false;
				//}

				if (performingOperations)
				{
					UpdateStatusText("Operations in progress! (Or the mod has crashed)");
					return false;
				}

				return true;
			}

			bool IsComponentMatch(Component c)
			{
				bool matchType, matchName;
				string compName, searchString;

				if (ignoreGenericTypes.Value.Value)
				{
					matchType = c.GetType().Name == componentField.Reference.Target?.GetType().Name;
				}
				else
				{
					matchType = c.GetType() == componentField.Reference.Target?.GetType();
				}

				compName = searchNiceName.Value.Value ? c.GetType().GetNiceName() : c.GetType().Name;
				compName = matchCase.Value.Value ? compName : compName.ToLower();

				searchString = matchCase.Value.Value ? nameField.Value.Value : nameField.Value.Value?.ToLower();

				matchName = searchString != null &&
							searchString.Trim() != "" &&
							(exactMatch.Value.Value ? compName == searchString.Trim() : compName.Contains(searchString.Trim()));

				if (componentField.Reference.Target == null || nameField.Value.Value == null || nameField.Value.Value.Trim() == "")
				{
					return matchType || matchName;
				}
				else
				{
					return matchType && matchName;
				}
			}

			List<Component> GetSearchComponents()
			{
				return searchRoot.Reference.Target?.GetComponentsInChildren((Component c) => IsComponentMatch(c));
			}

			ComponentSearchWizard(Slot x)
			{
				WizardSlot = x;
				WizardSlot.Tag = "Developer";
				WizardSlot.PersistentSelf = false;
				WizardSlot.LocalScale *= 0.00075f; 

				Slot Data = WizardSlot.AddSlot("Data");
				searchRoot = Data.AddSlot("searchRoot").AttachComponent<ReferenceField<Slot>>();
				searchRoot.Reference.Value = WizardSlot.World.RootSlot.ReferenceID;
				componentField = Data.AddSlot("componentField").AttachComponent<ReferenceField<Component>>();
				ignoreGenericTypes = Data.AddSlot("ignoreGenericTypes").AttachComponent<ValueField<bool>>();
				showDetails = Data.AddSlot("showDetails").AttachComponent<ValueField<bool>>();
				//confirmDestroy = Data.AddSlot("confirmDestroy").AttachComponent<ValueField<bool>>();
				nameField = Data.AddSlot("nameField").AttachComponent<ValueField<string>>();
				matchCase = Data.AddSlot("matchCase").AttachComponent<ValueField<bool>>();
				//allowChanges = Data.AddSlot("allowChanges").AttachComponent<ValueField<bool>>();
				searchNiceName = Data.AddSlot("searchNiceName").AttachComponent<ValueField<bool>>();
				maxResults = Data.AddSlot("maxResults").AttachComponent<ValueField<int>>();
				maxResults.Value.Value = 256;
				results = Data.AddSlot("referenceMultiplexer").AttachComponent<ReferenceMultiplexer<Component>>();
				exactMatch = Data.AddSlot("exactMatch").AttachComponent<ValueField<bool>>();

				UIBuilder UI = RadiantUI_Panel.SetupPanel(WizardSlot, WIZARD_TITLE.AsLocaleKey(), new float2(800f, 756f));
				RadiantUI_Constants.SetupEditorStyle(UI);

				UI.Canvas.MarkDeveloper();
				UI.Canvas.AcceptPhysicalTouch.Value = false;

				UI.SplitHorizontally(0.5f, out RectTransform left, out RectTransform right);

				left.OffsetMax.Value = new float2(-2f);
				right.OffsetMin.Value = new float2(2f);

				UI.NestInto(left);

				VerticalLayout verticalLayout = UI.VerticalLayout(4f, childAlignment: Alignment.TopCenter);
				verticalLayout.ForceExpandHeight.Value = false;

				UI.Style.MinHeight = 24f;
				UI.Style.PreferredHeight = 24f;
				UI.Style.PreferredWidth = 400f;
				UI.Style.MinWidth = 400f;

				UI.Text("Search Root:").HorizontalAlign.Value = TextHorizontalAlignment.Left;
				UI.Next("Root");
				UI.Current.AttachComponent<RefEditor>().Setup(searchRoot.Reference);

				UI.Spacer(24f);

				UI.Text("{A} Component Type:").HorizontalAlign.Value = TextHorizontalAlignment.Left;
				UI.Next("Component");
				UI.Current.AttachComponent<RefEditor>().Setup(componentField.Reference);

				UI.HorizontalElementWithLabel("Ignore Type Arguments:", 0.942f, () => UI.BooleanMemberEditor(ignoreGenericTypes.Value));

				UI.Spacer(24f);

				UI.Text("{B} Name Contains:").HorizontalAlign.Value = TextHorizontalAlignment.Left;

				var textField = UI.TextField();
				textField.Text.Content.OnValueChange += (field) => nameField.Value.Value = field.Value;

				UI.HorizontalElementWithLabel("Search Nice Name (With type arguments):", 0.942f, () => UI.BooleanMemberEditor(searchNiceName.Value));
				UI.HorizontalElementWithLabel("Match Case:", 0.942f, () => UI.BooleanMemberEditor(matchCase.Value));
				UI.HorizontalElementWithLabel("Exact Match:", 0.942f, () => UI.BooleanMemberEditor(exactMatch.Value));

				UI.Spacer(24f);

				UI.HorizontalElementWithLabel("Max Results:", 0.884f, () =>
				{
					var intField = UI.IntegerField(1, 1025);
					intField.ParsedValue.Value = maxResults.Value.Value;
					intField.ParsedValue.OnValueChange += (field) => maxResults.Value.Value = field.Value;
					return intField;
				});

				UI.HorizontalElementWithLabel("Spawn Detail Text:", 0.942f, () => UI.BooleanMemberEditor(showDetails.Value));

				searchButton = UI.Button("Search");
				searchButton.LocalPressed += SearchPressed;

				//UI.Text("----------");

				//UI.HorizontalElementWithLabel("Allow Changes:", 0.942f, () => UI.BooleanMemberEditor(allowChanges.Value));

				//enableButton = UI.Button("Enable");
				//enableButton.LocalPressed += EnablePressed;

				//disableButton = UI.Button("Disable");
				//disableButton.LocalPressed += DisablePressed;

				//UI.Spacer(24f);

				//UI.HorizontalElementWithLabel("Confirm Destroy:", 0.942f, () => UI.BooleanMemberEditor(confirmDestroy.Value));

				//destroyButton = UI.Button("Destroy");
				//destroyButton.LocalPressed += DestroyPressed;

				UI.Spacer(24f);

				UI.Text("Status:");
				statusText = UI.Text("---");

				UI.NestInto(right);
				UI.ScrollArea();
				UI.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);

				SyncMemberEditorBuilder.Build(results.References, "MatchingComponents", null, UI);

				WizardSlot.PositionInFrontOfUser(float3.Backward, distance: 1f);
			}

			void SearchPressed(IButton button, ButtonEventData eventData)
			{
				if (!ValidateWizard()) return;

				performingOperations = true;
				searchButton.Enabled = false;

				results.References.Clear();

				int count = 0;
				string text = "";
				bool stoppedEarly = false;

				foreach (Component c in GetSearchComponents())
				{
					if (results.References.Count >= maxResults.Value.Value)
					{
						stoppedEarly = true;
						break;
					}
					count++;
					text += $"<color=yellow>{c.GetType().GetNiceName()}</color>" + " - " + (c.Enabled ? "<color=green>Enabled</color>" : "<color=red>Disabled</color>") + " - " + $"<color=white>{GetSlotParentHierarchyString(c.Slot)}</color>" + "\n";
					results.References.Add(c);
				}

				if (stoppedEarly)
				{
					UpdateStatusText($"Found {count} matching components (Max Results limit reached).");
				}
				else
				{
					UpdateStatusText($"Found {count} matching components.");
				}

				if (showDetails.Value.Value && count > 0)
				{
					Slot textSlot = WizardSlot.LocalUserSpace.AddSlot("Search Text");
					UniversalImporter.SpawnText(textSlot, "Details", text, textSize: 12, canvasSize: new float2(1200, 400));
					textSlot.PositionInFrontOfUser();
				}

				performingOperations = false;
				searchButton.Enabled = true;
			}

			//void EnablePressed(IButton button, ButtonEventData eventData)
			//{
			//    if (!ValidateWizard()) return;

			//    if (!allowChanges.Value.Value)
			//    {
			//        UpdateStatusText("You must allow changes!");
			//        return;
			//    }

			//    if (results.References.Count == 0)
			//    {
			//        UpdateStatusText("No search results to process!");
			//        return;
			//    }

			//    performingOperations = true;
			//    enableButton.Enabled = false;

			//    int count = 0;
			//    WizardSlot.World.RunSynchronously(() =>
			//    {
			//        WizardSlot.World.BeginUndoBatch($"Enable {results.References.Count} Components");
			//        foreach (Component c in results.References)
			//        {
			//            if (c != null)
			//            {
			//                c.EnabledField.UndoableSet(true);
			//                count++;
			//            }
			//        }
			//        WizardSlot.World.EndUndoBatch();

			//        UpdateStatusText($"Enabled {count} matching components.");

			//        performingOperations = false;
			//        enableButton.Enabled = true;
			//    });
			//}

			//void DisablePressed(IButton button, ButtonEventData eventData)
			//{
			//    if (!ValidateWizard()) return;

			//    if (!allowChanges.Value.Value)
			//    {
			//        UpdateStatusText("You must allow changes!");
			//        return;
			//    }

			//    if (results.References.Count == 0)
			//    {
			//        UpdateStatusText("No search results to process!");
			//        return;
			//    }

			//    performingOperations = true;
			//    disableButton.Enabled = false;

			//    int count = 0;
			//    WizardSlot.World.RunSynchronously(() =>
			//    {
			//        WizardSlot.World.BeginUndoBatch($"Disable {results.References.Count} Components");
			//        foreach (Component c in results.References)
			//        {
			//            if (c != null)
			//            {
			//                c.EnabledField.UndoableSet(false);
			//                count++;
			//            }
			//        }
			//        WizardSlot.World.EndUndoBatch();

			//        UpdateStatusText($"Disabled {count} matching components.");

			//        performingOperations = false;
			//        disableButton.Enabled = true;
			//    });
			//}

			//void DestroyPressed(IButton button, ButtonEventData eventData)
			//{
			//    if (!ValidateWizard()) return;

			//    if (!allowChanges.Value.Value)
			//    {
			//        UpdateStatusText("You must allow changes!");
			//        return;
			//    }

			//    if (!confirmDestroy.Value.Value)
			//    {
			//        UpdateStatusText("You must confirm destroy!");
			//        return;
			//    }

			//    if (results.References.Count == 0)
			//    {
			//        UpdateStatusText("No search results to process!");
			//        return;
			//    }

			//    performingOperations = true;
			//    destroyButton.Enabled = false;

			//    int count = 0;
			//    WizardSlot.World.RunSynchronously(() =>
			//    {
			//        WizardSlot.World.BeginUndoBatch($"Destroy {results.References.Count} Components");
			//        foreach (Component c in results.References)
			//        {
			//            if (c != null)
			//            {
			//                c.UndoableDestroy();
			//                count++;
			//            }
			//        }
			//        WizardSlot.World.EndUndoBatch();

			//        UpdateStatusText($"Destroyed {count} matching components.");

			//        results.References.Clear();
			//        performingOperations = false;
			//        destroyButton.Enabled = true;
			//        confirmDestroy.Value.Value = false;
			//    });
			//}
		}
	}
}