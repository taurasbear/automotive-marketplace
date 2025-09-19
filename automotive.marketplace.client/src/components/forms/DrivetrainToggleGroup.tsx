import { getDrivetrainTypesOptions } from "@/api/enum/getDrivetrainTypesOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type DrivetrainToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const DrivetrainToggleGroup = ({
  className,
  ...props
}: DrivetrainToggleGroupProps) => {
  const { data: drivetrainTypesQuery } = useQuery(getDrivetrainTypesOptions);

  const drivetrainTypes = drivetrainTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {drivetrainTypes.map((drivetrain) => (
          <ToggleGroupItem
            key={drivetrain.drivetrainType}
            value={drivetrain.drivetrainType}
          >
            {drivetrain.drivetrainType}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default DrivetrainToggleGroup;
