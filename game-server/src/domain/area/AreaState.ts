import { Event } from '../../infr/Event';
import { EntityId, EntityState } from '../../infr/Entity';

interface AreaData {
  id: EntityId;
  type?: AreaType;
  subtype?: string;
  canUnitsWalkThoughtIt?: boolean;
  canUnitsShootThoughtIt?: boolean;
}

enum AreaType {
  LAKE = 'lake',
  MOUNTAIN = 'mountain',
  WIND_WALL = 'windWall'
}

class AreaState extends EntityState implements AreaData {
  public id: EntityId;
  public type: AreaType;
  public subtype: string;
  public canUnitsWalkThoughtIt: boolean;
  public canUnitsShootThoughtIt: boolean;

  public constructor (events: Array<Event<AreaData>>) {
    super();
    this.applyEvents(events);
  }
}

export {AreaState, AreaData, AreaType};
