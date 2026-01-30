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
		meshDataTool.CreateFromSurface(mesh, 0);
		GD.Print(meshDataTool.GetVertexCount());
		for(int i = 0; i<meshDataTool.GetVertexCount(); i++)
		{
			Vector3 vertexPosition = meshDataTool.GetVertex(i);
			CharacterBody3D vertex = vertexScene.Instantiate<CharacterBody3D>();
			AddChild(vertex);
			vertex.Position = vertexPosition;
			vertex.Scale *= 0.1f;
		}
		mouseRelative = new Vector2(0.0f, 0.0f);
	}

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseMotion)
		{
			mouseRelative = mouseMotion.Relative;
		}
    }
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		mouseRay();
		if(Input.IsActionPressed("left"))
		{
			Vector3 vertex = meshDataTool.GetVertex(0);
			vertex += new Vector3(-0.2f*(float)delta, 0.0f, 0.0f);
			meshDataTool.SetVertex(0, vertex);
			mesh.ClearSurfaces();
			meshDataTool.CommitToSurface(mesh);
			mesh.SurfaceSetMaterial(0, material);
			meshInstance3D.Mesh = mesh;
		}
		else if(Input.IsActionPressed("right"))
		{
			Vector3 vertex = meshDataTool.GetVertex(0);
			vertex += new Vector3(0.2f*(float)delta, 0.0f, 0.0f);
			meshDataTool.SetVertex(0, vertex);
			mesh.ClearSurfaces();
			meshDataTool.CommitToSurface(mesh);
			mesh.SurfaceSetMaterial(0, material);
			meshInstance3D.Mesh = mesh;
		}
	}

	private void mouseRay()
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
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
			GD.Print(raycastResult["collider"].GetType());
			if(raycastResult["collider"].GetType() == typeof(CharacterBody3D))
			{
				CharacterBody3D body = (CharacterBody3D)raycastResult["collider"];
				if(Input.IsMouseButtonPressed(0))
				{
					body.Velocity = new Vector3(mouseRelative.X, mouseRelative.Y, 0.0f);
					body.MoveAndSlide();
				}
			}
		}
	}
}
