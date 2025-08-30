import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllMakesOptions } from "../api/getAllMakesOptions";

type MakeSelectProps = {
  onValueChange: (value: string) => void;
};

const MakeSelect = ({ onValueChange }: MakeSelectProps) => {
  const { data: makesQuery } = useSuspenseQuery(getAllMakesOptions);

  const makes = makesQuery.data;

  return (
    <div>
      <Select defaultValue="all" onValueChange={onValueChange}>
        <SelectTrigger className="w-full">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Makes</SelectLabel>
            <SelectItem value="all">All makes</SelectItem>
            {makes.map((make) => (
              <SelectItem key={make.id} value={make.id}>
                {make.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default MakeSelect;
