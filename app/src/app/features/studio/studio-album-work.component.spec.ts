import { formatReviewRange } from './studio-album-work.component';

describe('formatReviewRange', () => {
  it('formats a point and a range in minutes and seconds', () => {
    expect(formatReviewRange(12500, 12500)).toBe('0:12');
    expect(formatReviewRange(12500, 18300)).toBe('0:12-0:18');
    expect(formatReviewRange(61000, 125000)).toBe('1:01-2:05');
  });
});
