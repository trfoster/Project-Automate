using ProjectAutomate.BuildingGrid;
using ProjectAutomate.FluidContainers;
using UnityEngine;

namespace ProjectAutomate
{
	public sealed class PipeBehaviour : BuiltObject, IFluidContainer
	{
		//bool smoothThroughput = true;
    
		[SerializeField] private float prevContent;
		[SerializeField] private int connectionCount;
		private float flowLeft;
		private float flowTop;
		[SerializeField] private float flowRate;
		private const float CSquared = 0.1f;
		private const float CSquaredDamper = 0.04f;
		private const float CFriction = 0f;
		private bool measureClamp;
		[SerializeField] private float flowClampedTick;
		public bool debug;
		[SerializeField] private bool enforceMinPipe; // if false assume enforceMaxPipe has been chosen
    
		private BuiltObject topObject;
		private BuiltObject rightObject;
		private BuiltObject leftObject;
		private BuiltObject bottomObject;

		private const float CMaxContent = 100f;

		public float GetMaxContent() {return CMaxContent;}

		private float content;
		public float GetContent() {return content;}

		public void SetContent(float content) {this.content = content;}

		private float moveLeft;
		public float GetMoveLeft() {return moveLeft;}

		private float moveTop;
		public float GetMoveTop() {return moveTop;}
    
		protected override void Setup()
		{
			TickSystem.OnFluidTick += FluidTickSystem_OnTick;
			BlockUpdate();
		}

		private void FluidTickSystem_OnTick(object sender, TickSystem.OnTickEventArgs eventArgs)
		{
			UpdateFlow();
			prevContent = content;
			UpdateContent();
			UpdateFlowRate();
			ColourUpdate();
		}

		private void BlockUpdate()
		{
			topObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x, pivotPos.y + 1).GetBuiltObject();
			rightObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x + 1, pivotPos.y).GetBuiltObject();
			leftObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x - 1, pivotPos.y).GetBuiltObject();
			bottomObject = GridBuildingSystem.Instance.grid.GetGridObject(pivotPos.x, pivotPos.y - 1).GetBuiltObject();

			connectionCount = 0;

			if (topObject is IFluidContainer) connectionCount++;
			if (rightObject is IFluidContainer) connectionCount++;
			if (leftObject is IFluidContainer) connectionCount++;
			if (bottomObject is IFluidContainer) connectionCount++;
		}

		private float FlowFunction(float content, float maxContent, float f)
		{
			float differentialPressure = PressureFunction(content, maxContent) - PressureFunction(this.content, PipeBehaviour.CMaxContent);
			float c = Sign(f) == Sign(differentialPressure) ? CSquared : CSquaredDamper;
			f += differentialPressure * c;
			f *= 1 - CFriction;
			//if (debug) Debug.Log(f);
			return f;
		}

		private static float PressureFunction(float content, float maxContent)
		{
			return content / maxContent * 100;
		}

		private float ClampFunction(float content, float maxContent, float f)
		{
			if (enforceMinPipe)
			{
				if (f > 0) f = ClampFlow(content, f, 0.25f * content);
				else if (f < 0) f = -ClampFlow(content, -f, 0.25f * content);
			}
			else
			{
				float r;
				if (f > 0)
				{
					r = CMaxContent - this.content;
					f = ClampFlow(r, f, 0.25f * r);

				}
				else if (f < 0)
				{
					r = maxContent - content;
					f = -ClampFlow(r, -f, 0.25f * r);
				}
			}
			return f;
		}

		private float ClampFlow(float content, float flow, float limit)
		{
			if (content <= 0f) return 0f;
			float n = Mathf.Max(0, flow - content);
			float a = Mathf.Max(0, flow - limit);
			float x = Mathf.Max(0, a - n);
			if (measureClamp)
			{
				flowClampedTick += x;
			}
			return flow <= limit ? flow : limit;
		}

		private static int Sign(float v)
		{
			return v > 0 ? 1 : v == 0 ? 0 : -1;
		}

		private void UpdateFlow()
		{
			float flow = flowTop;
			if (topObject is IFluidContainer topFluidContainer)
			{
				flow = FlowFunction(topFluidContainer.GetContent(), topFluidContainer.GetMaxContent(), flow);
				flowTop = ClampFunction(topFluidContainer.GetContent(), topFluidContainer.GetMaxContent(), flow);
				measureClamp = true;
				moveTop = ClampFunction(topFluidContainer.GetContent(), topFluidContainer.GetMaxContent(), flow);
				measureClamp = false;
			}

			if (!(topObject is IFluidContainer leftFluidContainer)) return;
			flow = flowLeft;
			flow = FlowFunction(leftFluidContainer.GetContent(), leftFluidContainer.GetMaxContent(), flow);
			flowLeft = ClampFunction(leftFluidContainer.GetContent(), leftFluidContainer.GetMaxContent(), flow);
			measureClamp = true;
			moveLeft = ClampFunction(leftFluidContainer.GetContent(), leftFluidContainer.GetMaxContent(), flow);
			measureClamp = false;
		}

		private void UpdateContent()
		{
			if (topObject is IFluidContainer topFluidContainer)
			{
				topFluidContainer.SetContent(topFluidContainer.GetContent() - moveTop);
				content += moveTop;
			}

			if (!(leftObject is IFluidContainer leftFluidContainer)) return;
			leftFluidContainer.SetContent(leftFluidContainer.GetContent() - moveLeft);
			content += moveLeft;
		}

		private void UpdateFlowRate()
		{
			float fp = 0f;
			float fn = 0f;
			void Add(float flow)
			{
				if (flow > 0f) fp += flow;
				else fn -= flow;
			}
			Add(moveTop);
			Add(moveLeft);
			if (rightObject is IFluidContainer rightFluidContainer)
			{
				Add(-rightFluidContainer.GetMoveLeft());
			}
			if (bottomObject is IFluidContainer bottomFluidContainer)
			{
				Add(-bottomFluidContainer.GetMoveTop());
			}
			flowRate = Mathf.Max(fp, fn);
		}

		private void ColourUpdate()
		{
			float contentNormalized = content / CMaxContent;
			float redScaled = contentNormalized * (-34f / 255f) + 34f / 255f;
			float greenScaled = contentNormalized * (85f / 255f) + 34f / 255f;
			float blueScaled = contentNormalized * (191f / 255f) + 34f / 255f;
			//Debug.Log("red: " + redNormalized);
			//Debug.Log("green: " + greenNormalized);
			//Debug.Log("blue: " + blueNormalized);
			//Debug.Log("content: " + contentNormalized);
			GetComponent<SpriteRenderer>().color = new Color(redScaled, greenScaled, blueScaled, 1f);
			//Debug.Log(thisSpriteRenderer.color);
		}
    
		private void OnDestroy()
		{
			TickSystem.OnAnimationTick -= FluidTickSystem_OnTick;
		}
	}
}