# ProjectS

2014년 전공 2학년 2학기 Unity Engine 기초 과목에서  
텀프로젝트로 개발한 프로젝트 입니다


목표는 **스타크래프트**를 **모작**하는 것이었습니다

플레이어는 일꾼을 컨트롤하여 자원을 채취할 수 있으며  
채취한 자원으로 유닛을 생성할 수 있습니다  
생성한 유닛을 컨트롤하여 적군을 섬멸할 수 있습니다  


구현한 기능은 크게 **자원시스템, 오브젝트풀 시스템, 전투시스템,  
컨트롤시스템, UI시스템, 이펙트,사운드,애니메이션 시스템**으로 나뉩니다

**< 자원시스템 >**  
맵상에는 미네랄이라는 자원이 존재하며    
플레이어는 일꾼을 활용하여 자원을 채취 할 수 있습니다  
플레이어는 채취한 자원을 소모하여 유닛을 생성할 수 있습니다  

**< 오브젝트풀 시스템 >**   
게임이 로딩될 때 각각의 유닛, 이펙트마다 다량을 생성한 후    
오브젝트풀을 만들어 유닛, 이펙트의 생성과 삭제를 관리합니다  

**< 전투시스템 >**   
유닛은 기본적으로 체력, 공격력, 방어력, 사거리, 이동속도 등의 속성을 가지며  
공격을 하여 자신의 공격력만큼 대상의 체력을 감소시킬 수 있습니다  
사거리 내의 적을 자동적으로 인식할 수 있으며, 인식하지 못한 먼 거리에서  
적이 공격을 가해도 자동적으로 인식하여 대응합니다  
당장 다수의 적이 사거리 내에 존재할 경우 가장 가까운 적부터 공격합니다  

**< 컨트롤시스템 >**  
플레이어는 유닛에게 이동, 공격, 중지, 미네랄채취 등의 명령을 내릴 수 있으며  
공격 명령의 경우 대상이 사거리 밖에 있을 경우 쫓아가서 공격을 합니다  
공격 명령의 대상이 특정 좌표인 경우 좌표까지 이동하며 만나는 적을 공격합니다

**< UI시스템 >**  
플레이어의 보유자원량, 유닛의 현재체력, 유닛의 속성정보, 이동/공격 등의 컨트롤버튼,  
드래그&선택, 선택된 부대표시, 마우스호버효과, 명령에 따른 커서효과 등을 표시합니다
              
**< 이펙트, 사운드, 애니메이션 >**  
유닛의 공격, 죽음 등에 대한 이펙트가 표시됩니다  
유닛의 응답, 공격, 죽음 등에 대한 사운드 효과가 있습니다  
유닛의 Idle, Attack, Walk 등에 따른 애니메이션이 있습니다


3D모델과 애니메이션은 가져온것입니다
유닛의 이동에는 Unity의 NavMesh를 사용하였습니다
유닛의 애니메이션에는 Unity의 FSM을 사용하였습니다 


이 프로젝트는 혼자 개발하였으며
개발기간은 약 5주정도 소요되었습니다


플레이영상은 여기서 보실 수 있습니다
https://www.youtube.com/watch?v=GST3hLrHQT0
