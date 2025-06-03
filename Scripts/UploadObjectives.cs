using UnityEngine;
using Firebase.Database;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;

//ENCARGADO DE CARGAR LOS OBJETEVOS DESDE FIREBASE COMPROBANDO SI HAY NUEVOS A AÑADIR O ANTIGUOS A ELIMINAR
public class UploadObjectives : MonoBehaviour
{
    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        // Llamar a CheckAndFixDependenciesAsync antes de utilizar Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                // Obtiene el UserId desde PlayerPrefs (asegúrate de que el usuario esté autenticado)
                userId = PlayerPrefs.GetString("UserId");

                UploadDefaultObjectives();//Actualizamos los objetivos
            }
            else
            {
                Debug.LogError("Firebase no está completamente inicializado. Error: " + task.Exception);
            }
        });
    }

    void UploadDefaultObjectives()
    {
        //Entramos en Firebase para obtener los datos de los Objetivos llamados "Items"
        dbReference.Child("users").Child(userId).Child("items").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al obtener objetivos del usuario: " + task.Exception);
                return;
            }

            //Firebase responde con ÉXITO
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result; //Contiene los objetivos actuales

                //Definición de nuevos objetivos 
                Dictionary<string, object> newObjectives = new Dictionary<string, object>()
                {
                     // Semana 1-2: Adaptación y hábito
                    { "item1", new Dictionary<string, object> { { "name", "Haz 10 flexiones y 20 sentadillas con peso corporal en un día" }, { "completed", false } } },
                    { "item2", new Dictionary<string, object> { { "name", "Camina 8,000 pasos al día durante 5 días seguidos" }, { "completed", false } } },
                    { "item3", new Dictionary<string, object> { { "name", "Corre sin parar 20 minutos" }, { "completed", false } } },
                    { "item4", new Dictionary<string, object> { { "name", "Completa 3 entrenamientos de cuerpo completo en una semana" }, { "completed", false } } },
                    { "item5", new Dictionary<string, object> { { "name", "Aguanta 30 segundos en plancha y repítelo tres veces en una semana" }, { "completed", false } } },
    
                    // Semana 3-4: Mayor resistencia y consistencia
                    { "item6", new Dictionary<string, object> { { "name", "Realiza 3 entrenamientos en una semana y añade 10 minutos de cardio extra en cada uno" }, { "completed", false } } },
                    { "item7", new Dictionary<string, object> { { "name", "Corre 2 km sin parar o camina 10,000 pasos diarios durante 7 días" }, { "completed", false } } },
                    { "item8", new Dictionary<string, object> { { "name", "Haz 15 flexiones y 30 sentadillas con peso corporal en un solo día" }, { "completed", false } } },
                    { "item9", new Dictionary<string, object> { { "name", "Aguanta 45 segundos en plancha y repítelo tres veces en una semana" }, { "completed", false } } },
                    { "item10", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, combinando fuerza y resistencia" }, { "completed", false } } },
    
                    // Semana 5-6: Introducción a cargas moderadas
                    { "item11", new Dictionary<string, object> { { "name", "Realiza 3 series de 10 repeticiones de peso muerto con un peso ligero en dos entrenamientos" }, { "completed", false } } },
                    { "item12", new Dictionary<string, object> { { "name", "Corre 3 km en menos de 25 minutos o haz 30 minutos de cardio sin parar" }, { "completed", false } } },
                    { "item13", new Dictionary<string, object> { { "name", "Haz 20 flexiones y 10 dominadas asistidas en una sesión" }, { "completed", false } } },
                    { "item14", new Dictionary<string, object> { { "name", "Aguanta 1 minuto en plancha y repítelo tres veces en una semana" }, { "completed", false } } },
                    { "item15", new Dictionary<string, object> { { "name", "Realiza una sesión de entrenamiento funcional con ejercicios de cuerpo entero" }, { "completed", false } } },
    
                    // Semana 7-8: Desafíos físicos y mejoras en rendimiento
                    { "item16", new Dictionary<string, object> { { "name", "Completa 4 entrenamientos en una semana, incluyendo un día de alta intensidad" }, { "completed", false } } },
                    { "item17", new Dictionary<string, object> { { "name", "Corre 5 km sin parar o camina 12,000 pasos diarios durante una semana" }, { "completed", false } } },
                    { "item18", new Dictionary<string, object> { { "name", "Realiza 25 flexiones y 15 dominadas asistidas en un solo día" }, { "completed", false } } },
                    { "item19", new Dictionary<string, object> { { "name", "Aguanta 90 segundos en plancha y repítelo tres veces en una semana" }, { "completed", false } } },
                    { "item20", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana y haz al menos 50 burpees en total" }, { "completed", false } } },

                    //LVL 5
                    // Semana 8-9: Consolidación de la base y aumento de intensidad
                    { "item21", new Dictionary<string, object> { { "name", "Realiza 20 flexiones y 30 sentadillas con peso corporal en un solo día" }, { "completed", false } } },
                    { "item22", new Dictionary<string, object> { { "name", "Camina 10,000 pasos al día durante 5 días seguidos" }, { "completed", false } } },
                    { "item23", new Dictionary<string, object> { { "name", "Completa 4 entrenamientos de cuerpo completo en una semana" }, { "completed", false } } },
                    { "item24", new Dictionary<string, object> { { "name", "Aguanta 1 minuto en plancha y repítelo tres veces en una semana" }, { "completed", false } } },
                    { "item25", new Dictionary<string, object> { { "name", "Realiza 3 series de 12 repeticiones de press de banca con mancuernas en dos entrenamientos" }, { "completed", false } } },

                    // Semana 9-10: Aumento de resistencia y fuerza con peso
                    { "item26", new Dictionary<string, object> { { "name", "Corre 5 km en menos de 30 minutos o haz 45 minutos de cardio continuo" }, { "completed", false } } },
                    { "item27", new Dictionary<string, object> { { "name", "Realiza 3 series de 10 repeticiones de sentadilla con barra con el 50% de tu peso corporal" }, { "completed", false } } },
                    { "item28", new Dictionary<string, object> { { "name", "Aguanta 90 segundos en plancha y repítelo cuatro veces en una semana" }, { "completed", false } } },
                    { "item29", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, combinando fuerza y resistencia" }, { "completed", false } } },
                    { "item30", new Dictionary<string, object> { { "name", "Realiza 5 dominadas estrictas o 15 asistidas en una sesión" }, { "completed", false } } },

                    // Semana 10-11: Carga progresiva y mejora en velocidad
                    { "item31", new Dictionary<string, object> { { "name", "Corre 6 km sin parar o camina 12,000 pasos diarios durante una semana" }, { "completed", false } } },
                    { "item32", new Dictionary<string, object> { { "name", "Realiza 3 series de 8 repeticiones de peso muerto con el 70% de tu peso corporal" }, { "completed", false } } },
                    { "item33", new Dictionary<string, object> { { "name", "Haz 30 flexiones y 20 dominadas asistidas en un solo día" }, { "completed", false } } },
                    { "item34", new Dictionary<string, object> { { "name", "Completa 3 entrenamientos en una semana con al menos 100 burpees en total" }, { "completed", false } } },
                    { "item35", new Dictionary<string, object> { { "name", "Aguanta 2 minutos en plancha y repítelo tres veces en una semana" }, { "completed", false } } },

                    // Semana 11-12: Resistencia avanzada y fuerza con carga alta
                    { "item36", new Dictionary<string, object> { { "name", "Realiza 10 dominadas estrictas en una sola sesión" }, { "completed", false } } },
                    { "item37", new Dictionary<string, object> { { "name", "Corre 8 km en menos de 45 minutos o haz 60 minutos de cardio sin parar" }, { "completed", false } } },
                    { "item38", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, incluyendo uno de alta intensidad" }, { "completed", false } } },
                    { "item39", new Dictionary<string, object> { { "name", "Realiza 3 series de 10 repeticiones de press de banca con el 80% de tu peso corporal" }, { "completed", false } } },
                    { "item40", new Dictionary<string, object> { { "name", "Aguanta 3 minutos en plancha y haz 20 flexiones con palmada en un solo día" }, { "completed", false } } },

                    //LVL 10
                    // Semana 12-13: Adaptación a cargas altas y resistencia sostenida
                    { "item41", new Dictionary<string, object> { { "name", "Realiza 3 series de 8 repeticiones de peso muerto con el 80% de tu peso corporal" }, { "completed", false } } },
                    { "item42", new Dictionary<string, object> { { "name", "Corre 8 km sin parar o haz 45 minutos de cardio de alta intensidad" }, { "completed", false } } },
                    { "item43", new Dictionary<string, object> { { "name", "Completa 4 entrenamientos en una semana, enfocando fuerza en tren superior e inferior" }, { "completed", false } } },
                    { "item44", new Dictionary<string, object> { { "name", "Realiza 10 dominadas estrictas en una sola sesión" }, { "completed", false } } },
                    { "item45", new Dictionary<string, object> { { "name", "Aguanta 2 minutos en plancha y repítelo tres veces en una semana" }, { "completed", false } } },

                    // Semana 13-14: Incremento en carga y volumen de trabajo
                    { "item46", new Dictionary<string, object> { { "name", "Realiza 3 series de 10 repeticiones de sentadilla con barra con el 80% de tu peso corporal" }, { "completed", false } } },
                    { "item47", new Dictionary<string, object> { { "name", "Corre 10 km en menos de 50 minutos o realiza 5 sprints de 200 metros a máxima velocidad" }, { "completed", false } } },
                    { "item48", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, incluyendo uno con énfasis en potencia" }, { "completed", false } } },
                    { "item49", new Dictionary<string, object> { { "name", "Realiza 3 series de 8 repeticiones de press de banca con el 85% de tu peso corporal" }, { "completed", false } } },
                    { "item50", new Dictionary<string, object> { { "name", "Aguanta 2 minutos en plancha lateral por lado y repítelo tres veces en la semana" }, { "completed", false } } },

                    // Semana 14-15: Enfoque en potencia y resistencia muscular
                    { "item51", new Dictionary<string, object> { { "name", "Realiza 3 series de 6 repeticiones de peso muerto con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item52", new Dictionary<string, object> { { "name", "Corre 12 km en menos de 60 minutos o realiza 10 sprints de 100 metros con descanso corto" }, { "completed", false } } },
                    { "item53", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, agregando un día de alta intensidad" }, { "completed", false } } },
                    { "item54", new Dictionary<string, object> { { "name", "Realiza 12 dominadas estrictas con lastre o 20 sin peso en una sola sesión" }, { "completed", false } } },
                    { "item55", new Dictionary<string, object> { { "name", "Aguanta 3 minutos en plancha y haz 30 flexiones con palmada en un solo día" }, { "completed", false } } },

                    // Semana 15-16: Resistencia extrema y máxima fuerza
                    { "item56", new Dictionary<string, object> { { "name", "Realiza 3 series de 5 repeticiones de sentadilla con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item57", new Dictionary<string, object> { { "name", "Corre 15 km sin parar o haz 60 minutos de intervalos de alta intensidad" }, { "completed", false } } },
                    { "item58", new Dictionary<string, object> { { "name", "Completa 6 entrenamientos en una semana, combinando fuerza, resistencia y potencia" }, { "completed", false } } },
                    { "item59", new Dictionary<string, object> { { "name", "Realiza 3 series de 5 repeticiones de press de banca con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item60", new Dictionary<string, object> { { "name", "Aguanta 4 minutos en plancha y realiza 50 burpees en menos de 3 minutos" }, { "completed", false } } },

                    //LVL 15
                    // Semana 1-2: Consolidación de fuerza máxima y resistencia avanzada
                    { "item61", new Dictionary<string, object> { { "name", "Realiza 3 series de 3 repeticiones de peso muerto con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item62", new Dictionary<string, object> { { "name", "Corre 15 km sin parar o realiza 45 minutos de entrenamiento HIIT intenso" }, { "completed", false } } },
                    { "item63", new Dictionary<string, object> { { "name", "Completa 5 entrenamientos en una semana, alternando fuerza, resistencia y explosividad" }, { "completed", false } } },
                    { "item64", new Dictionary<string, object> { { "name", "Realiza 15 dominadas estrictas o 10 con lastre en una sola sesión" }, { "completed", false } } },
                    { "item65", new Dictionary<string, object> { { "name", "Aguanta 3 minutos en plancha y repítelo tres veces en la semana" }, { "completed", false } } },

                    // Semana 3-4: Desarrollo de potencia y resistencia extrema
                    { "item66", new Dictionary<string, object> { { "name", "Realiza 3 series de 5 repeticiones de sentadilla con barra con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item67", new Dictionary<string, object> { { "name", "Corre 18 km en menos de 90 minutos o haz 5 sprints de 400 metros a máxima velocidad" }, { "completed", false } } },
                    { "item68", new Dictionary<string, object> { { "name", "Completa 6 entrenamientos en una semana, incluyendo uno de acondicionamiento metabólico" }, { "completed", false } } },
                    { "item69", new Dictionary<string, object> { { "name", "Realiza 3 series de 5 repeticiones de press de banca con el 90% de tu peso corporal" }, { "completed", false } } },
                    { "item70", new Dictionary<string, object> { { "name", "Aguanta 4 minutos en plancha y haz 40 flexiones con palmada en una sesión" }, { "completed", false } } },

                    // Semana 5-6: Desafíos de fuerza y resistencia avanzada
                    { "item71", new Dictionary<string, object> { { "name", "Realiza 3 series de 2 repeticiones de peso muerto con el 95% de tu peso corporal" }, { "completed", false } } },
                    { "item72", new Dictionary<string, object> { { "name", "Corre 21 km en menos de 1 hora y 45 minutos o realiza 10 sprints de 200 metros con descanso corto" }, { "completed", false } } },
                    { "item73", new Dictionary<string, object> { { "name", "Completa 6 entrenamientos en una semana, combinando fuerza máxima y resistencia" }, { "completed", false } } },
                    { "item74", new Dictionary<string, object> { { "name", "Realiza 20 dominadas estrictas en una sola sesión o 10 con 15 kg de lastre" }, { "completed", false } } },
                    { "item75", new Dictionary<string, object> { { "name", "Aguanta 5 minutos en plancha y haz 50 burpees en menos de 3 minutos" }, { "completed", false } } },

                    // Semana 7-8: Máximo rendimiento y pruebas finales
                    { "item76", new Dictionary<string, object> { { "name", "Realiza 3 series de 1 repetición de sentadilla con el 95% de tu peso corporal" }, { "completed", false } } },
                    { "item77", new Dictionary<string, object> { { "name", "Corre un maratón (42 km) o realiza 90 minutos de entrenamiento HIIT sin descanso prolongado" }, { "completed", false } } },
                    { "item78", new Dictionary<string, object> { { "name", "Completa 7 entrenamientos en una semana, incluyendo ejercicios de alta intensidad y resistencia" }, { "completed", false } } },
                    { "item79", new Dictionary<string, object> { { "name", "Realiza 3 series de 3 repeticiones de press de banca con el 95% de tu peso corporal" }, { "completed", false } } },
                    { "item80", new Dictionary<string, object> { { "name", "Aguanta 6 minutos en plancha y completa 100 burpees en menos de 6 minutos" }, { "completed", false } } }
                    //LVL 20
                };

                //Diccionario para guardar objetivos a añadir (se actualzia cada vez que se usa esta función)
                Dictionary<string, object> objectivesToAdd = new Dictionary<string, object>();

                // Comparar con los existentes y añadir solo los nuevos
                foreach (var obj in newObjectives)
                {
                    if (!snapshot.HasChild(obj.Key)) // Solo si el item no existe en Firebase lo añadimos al diccionario de solo los nuevos
                    {
                        objectivesToAdd[obj.Key] = obj.Value;
                    }
                }
                //AÑADIR NUEVOS OBJETIVOS:
                // Si hay objetivos a añadir en el diccionario de los nuevos
                if (objectivesToAdd.Count > 0)
                {
                    //Actualizamos el Firebase con los nuevos datos a Añadir
                    dbReference.Child("users").Child(userId).Child("items").UpdateChildrenAsync(objectivesToAdd).ContinueWith(updateTask =>
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Nuevos objetivos añadidos con éxito.");
                        }
                        else
                        {
                            Debug.LogError("Error al subir los nuevos objetivos: " + updateTask.Exception);
                        }
                    });
                }
                else
                {
                    Debug.Log("Todos los objetivos ya existen en Firebase.");
                }

                //ELIMINAR OBJETIVOS BORRADOS:
                //Para los items que stan en firebase, comprueba uno a uno
                foreach (var existingObjective in snapshot.Children)
                {
                    string existingKey = existingObjective.Key;

                    // Si el objetivo ya no está en la lista de nuevos objetivos, lo eliminamos
                    if (!newObjectives.ContainsKey(existingKey))
                    {
                        //Eliminar en Firebase
                        dbReference.Child("users").Child(userId).Child("items").Child(existingKey).RemoveValueAsync().ContinueWith(removeTask =>
                        {
                            if (removeTask.IsCompleted)
                            {
                                Debug.Log($"Objetivo {existingKey} eliminado con éxito.");
                            }
                            else
                            {
                                Debug.LogError("Error al eliminar el objetivo: " + removeTask.Exception);
                            }
                        });
                    }
                }
            }
        });
    }
}
