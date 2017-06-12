using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class ItemBuilt : MonoBehaviour
    {
        public void Update()
        {
            Queue<BeginItemConstruction> constructions = Multiplayer.client.getBeginningItemConstructions();

            while (constructions.Count > 0)
            {
                BeginItemConstruction construction = constructions.Dequeue();
                Console.WriteLine("Processing construction " + construction.ItemPosition + " " + construction.PlayerId);
                TechType techType;
                UWE.Utils.TryParseEnum<TechType>(construction.TechType, out techType);
                GameObject techPrefab = TechTree.main.GetGamePrefab(techType);
                Console.WriteLine("Built item is of tech " + techType.ToString());
                buildItem(ApiHelper.Vector3(construction.ItemPosition), ApiHelper.Quaternion(construction.Rotation), techType);
            }

            Queue<ConstructionAmountChanged> constructionAmountChanges = Multiplayer.client.getConstrutionAmountChanged();

            while (constructionAmountChanges.Count > 0)
            {
                ConstructionAmountChanged amountChanged = constructionAmountChanges.Dequeue();
                Console.WriteLine("Processing ConstructionAmountChanged " + amountChanged.ItemPosition + " " + amountChanged.PlayerId + " " + amountChanged.ConstructionAmount);
                
                Constructable[] constructables = GameObject.FindObjectsOfType<Constructable>();

                Console.WriteLine("constructables " + constructables.Length);

                foreach (Constructable constructable in constructables)
                {
                    if (constructable.transform.position == ApiHelper.Vector3(amountChanged.ItemPosition))
                    {
                        Console.WriteLine("Found constructable!");
                        constructable.constructedAmount = amountChanged.ConstructionAmount;
                        constructable.Construct();
                    }
                }
                
                /*
                Collider[] colliders = Physics.OverlapSphere(ApiHelper.Vector3(amountChanged.Location), 100f);
                Console.WriteLine("Total colliders: " + colliders.Length);
                foreach(Collider collider in colliders)
                {
                    Constructable constructable = collider.gameObject.GetComponent<Constructable>();

                    if (constructable != null)
                    {
                        Console.WriteLine("Found constructable!");
                        constructable.constructedAmount = amountChanged.ConstructionAmount;
                    }

                    Base base1 = collider.gameObject.GetComponent<Base>();

                    if (base1 != null)
                    {
                        ConstructableBase constructableBase = base1.GetComponentInParent<ConstructableBase>();

                        Console.WriteLine("Found constructableBase!");
                        constructableBase.constructedAmount = amountChanged.ConstructionAmount;
                    }
                }*/
            }
        }

        public void buildItem(Vector3 position, Quaternion rotation, TechType techType)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);
            MultiplayerBuilder.TryPlace();

            ///SubRoot currentSub = Player.main.GetCurrentSub();
            // GameObject prefab = TechTree.main.GetGamePrefab(techType);
            //  Console.WriteLine("Got prefab for object");

            // GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(prefab, position, rotation);
            //gameObject.SetActive(true);

            /*  bool flag = false;
              bool flag2 = false;
              if (currentSub != null)
              {
                  flag = currentSub.isBase;
                  flag2 = currentSub.isCyclops;
                  gameObject.transform.parent = currentSub.GetModulesRoot();
              }

              gameObject.transform.position = position;
              gameObject.transform.rotation = rotation;
              //Constructable construtable = gameObject.GetComponentInParent<Constructable>();
              //construtable.SetState(false, true);
              //global::Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer((!flag) ? "Interior" : "Default"), true, -1);
              //construtable.SetIsInside(flag || flag2);

              gameObject.SetActive(true);*/
        }
    }
}
