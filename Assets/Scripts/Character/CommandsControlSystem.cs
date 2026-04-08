using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HEAVYART.TopDownShooter.Netcode
{
    public class CommandsControlSystem : NetworkBehaviour
    {
        public List<CommandScriptableObject> activeCommands { get; private set; } = new List<CommandScriptableObject>();

        private void FixedUpdate()
        {
            activeCommands.ForEach(activeCommand => activeCommand.FixedUpdate());

            //Remove all expired modifiers
            RemoveCompleteCommands();
        }

        [Rpc(SendTo.Everyone)]
        public void ReceiveCommandsRpc(string serializedCommands)
        {
            List<CommandScriptableObject> receivedCommands = JsonConvert.DeserializeObject<List<CommandScriptableObject>>(serializedCommands);

            ReceiveCommands(receivedCommands);
        }

        public void ReceiveCommands(List<CommandScriptableObject> commands)
        {
            //Receive

            //Modifiers
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i] is ModifierScriptableObject modifier)
                {
                    modifier.Activate(this);

                    CombineModifiers(modifier);

                    activeCommands.Add(modifier);
                }
            }

            //Commands
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i] is LogicScriptableObject logic)
                {
                    logic.Activate(this);

                    activeCommands.Add(logic);
                }
            }

            RemoveCompleteCommands();
        }

        private void RemoveCompleteCommands()
        {
            activeCommands.RemoveAll(activeCommand =>
            {
                if (activeCommand.isComplete == true)
                    Destroy(activeCommand);

                return activeCommand.isComplete;
            });
        }

        public void Clear()
        {
            activeCommands.ForEach(activeCommand => activeCommand.Complete());
            RemoveCompleteCommands();
        }

        private void CombineModifiers(ModifierScriptableObject modifier)
        {
            for (int i = 0; i < activeCommands.Count; i++)
            {
                if (activeCommands[i].isComplete == true) continue;

                if (activeCommands[i].type == modifier.type)
                {
                    ModifierScriptableObject existingModifier = activeCommands[i] as ModifierScriptableObject;

                    if (existingModifier.TryMergeModifiers(modifier) == true)
                        modifier.Complete();
                }
            }
        }

        public ModifierScriptableObject GetModifier<T>() where T : ModifierScriptableObject
        {
            for (int i = 0; i < activeCommands.Count; i++)
                if (activeCommands[i] is T modifier)
                    return modifier;

            return null;
        }

        public override void OnDestroy()
        {
            Clear();
        }
    }
}
