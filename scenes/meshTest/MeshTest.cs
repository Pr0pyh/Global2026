using Godot;
using System;

public partial class MeshTest : Node3D
{
	[Export]
	MeshInstance3D meshInstance3D;
	[Export]
	Camera3D camera;
	[Export]
	PackedScene vertexScene;
	ArrayMesh mesh;
	[Export]
	Material material;
	MeshDataTool meshDataTool;
	Godot.Collections.Array<CharacterBody3D> vertices;
	Vector2 mouseRelative;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, ((QuadMesh)meshInstance3D.Mesh).GetMeshArrays());
		meshDataTool = new MeshDataTool();
		vertices = new Godot.Collections.Array<CharacterBody3D>();
		meshDataTool.CreateFromSurface(mesh, 0);
		GD.Print(meshDataTool.GetVertexCount());
		for(int i = 0; i<meshDataTool.GetVertexCount(); i++)
		{
			Vector3 vertexPosition = meshDataTool.GetVertex(i);
			CharacterBody3D vertex = vertexScene.Instantiate<CharacterBody3D>();
			AddChild(vertex);
			vertex.Position = vertexPosition;
			vertex.Scale *= 0.1f;
			vertices.Add(vertex);
		}
		mouseRelative = new Vector2(0.0f, 0.0f);
	}

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseMotion)
		{
			if(mouseMotion.Relative != new Vector2(0.0f, 0.0f)) mouseRelative = mouseMotion.Relative;
		}
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		mouseRay();
		updateMesh();
	}

	private void updateMesh()
	{
		for(int i = 0; i<meshDataTool.GetVertexCount(); i++)
		{
			Vector3 newVertexPosition = vertices[i].GlobalPosition;
			meshDataTool.SetVertex(i, newVertexPosition);
			mesh.ClearSurfaces();
			meshDataTool.CommitToSurface(mesh);
			mesh.SurfaceSetMaterial(0, material);
			meshInstance3D.Mesh = mesh;
		}
	}

	private void mouseRay()
	{
		Godot.Vector2 mousePos = GetViewport().GetMousePosition();
		float rayLength = 3000;
		var from = camera.ProjectRayOrigin(mousePos);
		var to = camera.ProjectRayNormal(mousePos) * rayLength;
		var space = GetWorld3D().DirectSpaceState;
		var rayQuery = new PhysicsRayQueryParameters3D();
		rayQuery.From = from;
		rayQuery.To = to;
		var raycastResult = space.IntersectRay(rayQuery);
		if(raycastResult.Count != 0)
		{
			GodotObject collider = (GodotObject)raycastResult["collider"];
			if(typeof(CharacterBody3D).IsAssignableFrom(collider.GetType()))
			{
				CharacterBody3D body = (CharacterBody3D)raycastResult["collider"];
				if(Input.IsMouseButtonPressed(MouseButton.Left))
				{
					Godot.Vector3 mousePosition3D = (Godot.Vector3)raycastResult["position"];
					body.GlobalPosition = new Godot.Vector3(mousePosition3D.X, mousePosition3D.Y, 0.0f);
				}
			}
		}
	}
}
