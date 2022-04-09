Contact/Feedback/Comment:	
monsieur.nez@gmail.com
http://monsieurnez.blogspot.ca/


Build with Unity version: 3.4.2f3
Version 1.0


Installation:
-Import the package in your project
-Must be in the YOUR_PROJECT/Assets/Editor folder


-The new creation menu will be in the GameObject/Create Geometry
-When you click on the "Create XXX" button, it will create an object at the first intersection found with a collider, if no intersection are found,
the object will be created at the origin [0, 0, 0]
-All the created object will contain a mesh renderer with a default diffuse and a collider
-For the Cylinder/Cone, you can specify 2 different subdivision (one for the render and one for the collider)
-The smooth normal option (for the Cylinder/Cone) create more vertex when it's disabled